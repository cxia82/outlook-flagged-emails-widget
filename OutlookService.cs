using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NotificationWidget
{
    public record FlaggedEmail(string Subject, string Sender, string EntryID, string StoreID);

    public static class OutlookService
    {
        public static void OpenEmail(string entryId, string storeId)
        {
            try
            {
                var type = Type.GetTypeFromProgID("Outlook.Application");
                if (type == null) return;
                dynamic? outlook = Activator.CreateInstance(type);
                if (outlook == null) return;
                dynamic mail = outlook.GetNamespace("MAPI").GetItemFromID(entryId, storeId);
                mail.Display(false);
            }
            catch { }
        }

        public static List<FlaggedEmail> GetFlaggedEmails()
        {
            var results = new List<FlaggedEmail>();
            dynamic? outlook = null;
            try
            {
                var type = Type.GetTypeFromProgID("Outlook.Application");
                if (type == null) return results;

                outlook = Activator.CreateInstance(type);
                if (outlook == null) return results;
                dynamic ns = outlook.GetNamespace("MAPI");
                ns.Logon(Type.Missing, Type.Missing, false, true);
                dynamic inbox = ns.GetDefaultFolder(6);
                dynamic items = inbox.Items;
                items.Sort("[ReceivedTime]", true);

                int total = (int)items.Count;
                int found = 0;
                for (int i = 1; i <= Math.Min(total, 500); i++)
                {
                    try
                    {
                        dynamic item = items[i];
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
                        Marshal.ReleaseComObject(item);
                    }
                    catch { }
                }
                Marshal.ReleaseComObject(items);
                Marshal.ReleaseComObject(inbox);
                Marshal.ReleaseComObject(ns);
            }
            catch (Exception ex)
            {
                results.Add(new FlaggedEmail("Error: " + ex.GetType().Name, ex.Message.Length > 60 ? ex.Message[..60] : ex.Message, "", ""));
            }
            finally
            {
                if (outlook != null) try { Marshal.ReleaseComObject(outlook); } catch { }
            }
            return results;
        }
    }
}
