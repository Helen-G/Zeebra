using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Player.ApplicationServices.EventHandlers.ActivityLog
{
    public class PlayerRecordEditActivityLogEventHandlers : PlayerActivityLogEventHandlersBase
    {
        public PlayerRecordEditActivityLogEventHandlers(IUnityContainer container)
            : base(container)
        {
            Category = "PlayerRecordEdit";
        }

        public void Handle(PlayerRegistered @event)
        {
            AddPlayerLog(@event.PlayerId);
        }

        private void AddPlayerLog(Guid playerId)
        {
            var playerCommands = Container.Resolve<PlayerCommands>();
            playerCommands.AddPlayerInfoLogRecord(playerId);
        }

        public void Handle(PlayerUpdated @event)
        {
            var playerId = @event.Player.Id;
            AddPlayerLog(playerId);

            var playerQueries = Container.Resolve<PlayerQueries>();
            var lastPlayerInfoRecords = playerQueries.GetPlayerInfoLog(playerId).Take(2).ToArray();
            var newValues = lastPlayerInfoRecords[0];
            var oldValues = lastPlayerInfoRecords[1];

            var changes = GetPlayerFieldChanges(oldValues, newValues).ToList();

            AddActivityLog(String.Join(", ", changes.Select(c => c.Field)), @event, @event.Player.Id,
                changes.ToDictionary(c => c.Field, c => (object) (c.OldValue + " -> " + c.NewValue)));
        }

        private static IEnumerable<PlayerFieldChange> GetPlayerFieldChanges(PlayerInfoLog oldValues, PlayerInfoLog newValues)
        {
            var changes = new List<PlayerFieldChange>();
            var customLogProperties = new[] { "Id", "Player", "RowVersion" };
            typeof (PlayerInfoLog).GetProperties()
                .Where(pi => !customLogProperties.Contains(pi.Name))
                .ForEach(pi =>
                {
                    var oldValue = pi.GetValue(oldValues) != null ? pi.GetValue(oldValues).ToString() : null;
                    var newValue = pi.GetValue(newValues) != null ? pi.GetValue(newValues).ToString() : null;
                    if (oldValue != newValue)
                    {
                        changes.Add(new PlayerFieldChange
                        {
                            Field = pi.Name,
                            OldValue = oldValue,
                            NewValue = newValue
                        });
                    }
                });
            return changes;
        }

        private class PlayerFieldChange
        {
            public string Field { get; set; }
            public string OldValue { get; set; }
            public string NewValue { get; set; }
        }
    }
}
