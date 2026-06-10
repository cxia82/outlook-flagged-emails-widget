using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NotificationWidget
{
    public record InboxSummary(List<FlaggedEmail> FlaggedEmails, int UnreadCount);

    public record FlaggedEmail(string Subject, string Sender, string EntryID, string StoreID);

    public static class OutlookService
    {
        public static void OpenEmail(string entryId, string storeId)
        {
            dynamic? outlook = null;
            dynamic? ns = null;
            dynamic? mail = null;
            try
            {
                var type = Type.GetTypeFromProgID("Outlook.Application");
                if (type == null) return;
                outlook = Activator.CreateInstance(type);
                if (outlook == null) return;
                ns = outlook.GetNamespace("MAPI");
                mail = ns.GetItemFromID(entryId, storeId);
                mail.Display(false);
            }
            catch (Exception ex)
            {
                StartupPerfLog.Write($"OpenEmail failed ({ex.GetType().Name})");
            }
            finally
            {
                ReleaseComObjectSafe(mail);
                ReleaseComObjectSafe(ns);
                ReleaseComObjectSafe(outlook);
            }
        }

        public static List<FlaggedEmail> GetFlaggedEmails()
        {
            return GetInboxSummary().FlaggedEmails;
        }

        public static int GetUnreadInboxCount()
        {
            return GetInboxSummary().UnreadCount;
        }

        public static InboxSummary GetInboxSummary(int maxEmailsToCheck = 1000)
        {
            var results = new List<FlaggedEmail>();
            int unreadCount = 0;
            int scanLimit = Math.Clamp(maxEmailsToCheck, 100, 5000);
            dynamic? outlook = null;
            dynamic? ns = null;
            dynamic? inbox = null;
            dynamic? items = null;
            try
            {
                var type = Type.GetTypeFromProgID("Outlook.Application");
                if (type == null) return new InboxSummary(results, unreadCount);

                outlook = Activator.CreateInstance(type);
                if (outlook == null) return new InboxSummary(results, unreadCount);
                ns = outlook.GetNamespace("MAPI");
                ns.Logon(Type.Missing, Type.Missing, false, true);
                inbox = ns.GetDefaultFolder(6);
                try { unreadCount = (int)inbox.UnReadItemCount; } catch { unreadCount = 0; }
                items = inbox.Items;
                items.Sort("[ReceivedTime]", true);

                int total = (int)items.Count;
                int found = 0;
                for (int i = 1; i <= Math.Min(total, scanLimit); i++)
                {
                    dynamic? item = null;
                    try
                    {
                        item = items[i];
                        int flagStatus = 0;
                        try { flagStatus = (int)item.FlagStatus; } catch { }
                        if (flagStatus == 2)
                        {
                            string subject = "", sender = "", entryId = "", storeId = "";
                            try { subject = (string)(item.Subject ?? "(No subject)"); } catch { }
                            try { sender = (string)(item.SenderName ?? ""); } catch { }
                            try { entryId = (string)(item.EntryID ?? ""); } catch { }
                            try { storeId = (string)(item.Parent.StoreID ?? ""); } catch { }
                            results.Add(new FlaggedEmail(subject, sender, entryId, storeId));
                            if (++found >= 10) break;
                        }
                    }
                    catch { }
                    finally
                    {
                        ReleaseComObjectSafe(item);
                    }
                }
            }
            catch (Exception ex)
            {
                StartupPerfLog.Write($"GetInboxSummary failed ({ex.GetType().Name})");
                results.Add(new FlaggedEmail("Error: " + ex.GetType().Name, ex.Message.Length > 60 ? ex.Message[..60] : ex.Message, "", ""));
            }
            finally
            {
                ReleaseComObjectSafe(items);
                ReleaseComObjectSafe(inbox);
                ReleaseComObjectSafe(ns);
                ReleaseComObjectSafe(outlook);
            }
            return new InboxSummary(results, unreadCount);
        }

        private static void ReleaseComObjectSafe(object? comObject)
        {
            if (comObject == null) return;
            try
            {
                Marshal.ReleaseComObject(comObject);
            }
            catch
            {
            }
        }
    }
}
