using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using Microsoft.Practices.ServiceLocation;

namespace AFT.RegoV2.Infrastructure.SessionState
{
    public static class HttpContextEx
    {
        public static string GetApplicationName(this HttpContext context)
        {
            var physicalPath = context.Request.PhysicalApplicationPath;

            if (string.IsNullOrWhiteSpace(physicalPath))
            {
                return string.Empty;
            }

            string fullPath = Path.GetFullPath(physicalPath).TrimEnd(Path.DirectorySeparatorChar);
            string applicationName = fullPath.Split(Path.DirectorySeparatorChar).Last();

            return applicationName;
        }
    }

    public class PersistableSessionStoreProvider : SessionStateStoreProviderBase
    {
        private SessionStateSection SessionStateConfig { get; set; }

        private void RemoveExpiredSessions()
        {
            var repository = ServiceLocator.Current.GetInstance<ISecurityRepository>();

            var sessions = repository.Sessions.Where(s => s.ExpireDate < DateTimeOffset.Now);

            foreach (var session in sessions)
            {
                repository.Sessions.Remove(session);
            }

            repository.SaveChanges();
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            SessionStateConfig = (SessionStateSection)(ConfigurationManager.GetSection("system.web/sessionState"));
        }

        public override void Dispose()
        {
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return false;
        }

        public override void InitializeRequest(HttpContext context)
        {

        }

        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId,
            out SessionStateActions actions)
        {
            return GetSessionStoreItem(false, context, id, out locked,
              out lockAge, out lockId, out actions);
        }

        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge,
            out object lockId, out SessionStateActions actions)
        {
            return GetSessionStoreItem(false, context, id, out locked,
              out lockAge, out lockId, out actions);
        }

        //
        // GetSessionStoreItem is called by both the GetItem and 
        // GetItemExclusive methods. GetSessionStoreItem retrieves the 
        // session data from the data source. If the lockRecord parameter
        // is true (in the case of GetItemExclusive), then GetSessionStoreItem
        // locks the record and sets a new LockId and LockDate.
        //

        private SessionStateStoreData GetSessionStoreItem(bool lockRecord,
          HttpContext context,
          string id,
          out bool locked,
          out TimeSpan lockAge,
          out object lockId,
          out SessionStateActions actionFlags)
        {
            RemoveExpiredSessions();

            var repository = ServiceLocator.Current.GetInstance<ISecurityRepository>();

            var applicationName = context.GetApplicationName();

            // Initial values for return value and out parameters.
            SessionStateStoreData item = null;
            lockAge = TimeSpan.Zero;
            lockId = null;
            locked = false;
            actionFlags = 0;

            var session = repository.Sessions
                .SingleOrDefault(
                    s => s.SessionId == id && s.ApplicationName == applicationName);

            if (session != null)
            {
                if (lockRecord)
                {
                    session.Locked = false;
                    session.LockDate = DateTimeOffset.Now;
                }

                var serializedItems = session.SessionItems;
                lockId = session.LockId;
                lockAge = DateTime.Now - session.LockDate;
                actionFlags = (SessionStateActions)session.Flags;
                var timeout = session.Timeout;

                item = actionFlags == SessionStateActions.InitializeItem ? CreateNewStoreData(context, (int)SessionStateConfig.Timeout.TotalMinutes) :
                    Deserialize(context, serializedItems, timeout);
            }

            return item;
        }


        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            RemoveExpiredSessions();

            var repository = ServiceLocator.Current.GetInstance<ISecurityRepository>();

            var applicationName = context.GetApplicationName();

            var session =
                    repository.Sessions.SingleOrDefault(
                        s =>
                            s.SessionId == id && s.ApplicationName == applicationName &&
                            s.LockId == (int)lockId);

            if (session != null)
            {
                session.LockId = 0;
                session.ExpireDate = DateTimeOffset.Now.AddMinutes((int)SessionStateConfig.Timeout.TotalMinutes);

                repository.SaveChanges();
            }
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            RemoveExpiredSessions();

            var repository = ServiceLocator.Current.GetInstance<ISecurityRepository>();

            // Serialize the SessionStateItemCollection as a string.
            var sessionItems = Serialize((SessionStateItemCollection)item.Items);

            var session =
                repository.Sessions.SingleOrDefault(
                    s => s.SessionId == id);

            if (newItem)
            {
                if (session != null)
                {
                    repository.Sessions.Remove(session);
                }

                var applicationName = context.GetApplicationName();
                
                var newSession = new Session
                {
                    Id = Guid.NewGuid(),
                    SessionId = id,
                    ApplicationName = applicationName,
                    CreatedDate = DateTimeOffset.Now,
                    ExpireDate = DateTimeOffset.Now.AddMinutes(item.Timeout),
                    LockDate = DateTimeOffset.Now,
                    LockId = 0,
                    Timeout = item.Timeout,
                    Locked = false,
                    SessionItems = sessionItems,
                    Flags = 0
                };

                repository.Sessions.Add(newSession);
            }
            else
            {
                if (session != null)
                {
                    session.ExpireDate = DateTimeOffset.Now.AddMinutes(item.Timeout);
                    session.SessionItems = sessionItems;
                    session.Locked = false;
                    session.Flags = 0;
                }
            }

            repository.SaveChanges();
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            var repository = ServiceLocator.Current.GetInstance<ISecurityRepository>();

            var applicationName = context.GetApplicationName();

            var session =
                    repository.Sessions.SingleOrDefault(
                        s =>
                            s.SessionId == id && s.ApplicationName == applicationName &&
                            s.LockId == (int)lockId);

            if (session != null)
            {
                repository.Sessions.Remove(session);

                repository.SaveChanges();
            }
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            var repository = ServiceLocator.Current.GetInstance<ISecurityRepository>();

            var applicationName = context.GetApplicationName();

            var session =
                repository.Sessions.SingleOrDefault(
                    s =>
                        s.SessionId == id && s.ApplicationName == applicationName);

            if (session != null)
            {
                session.ExpireDate = DateTimeOffset.Now.AddMinutes(SessionStateConfig.Timeout.TotalMinutes);

                repository.SaveChanges();
            }
        }

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return new SessionStateStoreData(new SessionStateItemCollection(),
              SessionStateUtility.GetSessionStaticObjects(context),
              timeout);
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            var repository = ServiceLocator.Current.GetInstance<ISecurityRepository>();

            var applicationName = context.GetApplicationName();

            var session = new Session
            {
                Id = Guid.NewGuid(),
                SessionId = id,
                ApplicationName = applicationName,
                CreatedDate = DateTimeOffset.Now,
                ExpireDate = DateTimeOffset.Now.AddMinutes(timeout),
                LockDate = DateTimeOffset.Now,
                LockId = 0,
                Timeout = timeout,
                Locked = false,
                SessionItems = string.Empty,
                Flags = 1
            };

            repository.Sessions.Add(session);

            repository.SaveChanges();
        }

        public override void EndRequest(HttpContext context)
        {

        }

        private string Serialize(SessionStateItemCollection items)
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            if (items != null)
                items.Serialize(writer);

            writer.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        private SessionStateStoreData Deserialize(HttpContext context,
          string serializedItems, int timeout)
        {
            var ms =
              new MemoryStream(Convert.FromBase64String(serializedItems));

            var sessionItems =
              new SessionStateItemCollection();

            if (ms.Length > 0)
            {
                var reader = new BinaryReader(ms);
                sessionItems = SessionStateItemCollection.Deserialize(reader);
            }

            return new SessionStateStoreData(sessionItems,
              SessionStateUtility.GetSessionStaticObjects(context),
              timeout);
        }

    }
}
