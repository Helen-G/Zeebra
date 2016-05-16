using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Services.Security;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Player.ApplicationServices
{
    public class PlayerActivityLogEventHandlersBase : MarshalByRefObject
    {
        protected readonly IUnityContainer Container;

        protected string Category;

        protected PlayerActivityLogEventHandlersBase(
            IUnityContainer container
            )
        {
            Container = container;
        }

        private string GetUserName(string performedBy)
        {
            var security = Container.Resolve<ISecurityRepository>();

            var user = security.Users.SingleOrDefault(u => u.Username != null && u.Username.Equals(performedBy, StringComparison.InvariantCultureIgnoreCase));
            var username = user != null ? user.Username : performedBy;

            return string.IsNullOrWhiteSpace(username) ? "System" : username;
        }

        protected string GetPlayerName(Guid playerId)
        {
            var repository = Container.Resolve<IPlayerRepository>();
            var player = repository.Players.SingleOrDefault(p => p.Id == playerId);

            return player == null ? string.Empty : player.Username;
        }

        protected void AddActivityLog(string activityName, IDomainEvent @event, Guid playerId, string performedBy, 
            string remarks)
        {
            var repository = Container.Resolve<IPlayerRepository>();

            repository.PlayerActivityLog.Add(new PlayerActivityLog
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Category = Category,
                ActivityDone = activityName,
                DatePerformed = @event.EventCreated,
                PerformedBy = performedBy,
                Remarks = remarks ?? string.Empty
            });
            repository.SaveChanges();
        }

        protected void AddActivityLog(string activityName, IDomainEvent @event, Guid playerId,
            Dictionary<string, object> activityValues)
        {
            AddActivityLog(activityName, @event, playerId, activityValues == null ? string.Empty :
                    String.Join("; ", activityValues.Select(v => v.Key + ": " + v.Value)));
        }

        protected void AddActivityLog(string activityName, IDomainEvent @event, Guid playerId)
        {
            AddActivityLog(activityName, @event, playerId, GetUserName(@event.EventCreatedBy), string.Empty);
        }

        protected void AddActivityLog(string activityName, IDomainEvent @event, Guid playerId, string remarks)
        {
            AddActivityLog(activityName, @event, playerId, GetUserName(@event.EventCreatedBy), remarks);
        }
    }
}
