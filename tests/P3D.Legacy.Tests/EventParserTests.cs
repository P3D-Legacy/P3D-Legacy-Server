using NUnit.Framework;

using P3D.Legacy.Common.PlayerEvents;

using System.Globalization;

namespace P3D.Legacy.Tests
{
    internal sealed class EventParserTests
    {
        // achieved the emblem "{emblem}"!
        // hosted a battle: "Player {playername} got defeated by Player {trainername}".
        // evolved their {pokemonname} into a {pokemonname}!
        // got defeated by {trainertype} {trainername}.
        // got defeated by a wild {pokemonname}.

        [Test]
        public void TestAchievedEmblemEvent()
        {
            const string arg0 = "Test";
            const string msgTemplate = "achieved the emblem \"{0}\"!";
            var msg = string.Format(CultureInfo.InvariantCulture, msgTemplate, arg0);
            Assert.True(PlayerEventParser.TryParse(msg, out var @event));
            Assert.IsInstanceOf<AchievedEmblemEvent>(@event);
            Assert.AreEqual(PlayerEventType.AchievedEmblem, @event!.EventType);
            Assert.AreEqual(arg0, ((AchievedEmblemEvent) @event).Emblem);
        }

        [Test]
        public void TestHostedABattleEvent()
        {
            const string arg0 = "Test";
            const string arg1 = "Test2";
            const string msgTemplate = "hosted a battle: \"Player {0} got defeated by Player {1}\".";
            var msg = string.Format(CultureInfo.InvariantCulture, msgTemplate, arg0, arg1);
            Assert.True(PlayerEventParser.TryParse(msg, out var @event));
            Assert.IsInstanceOf<HostedABattleEvent>(@event);
            Assert.AreEqual(PlayerEventType.HostedABattle, @event!.EventType);
            Assert.AreEqual(arg0, ((HostedABattleEvent) @event).DefeatedTrainer);
            Assert.AreEqual(arg1, ((HostedABattleEvent) @event).Trainer);
        }

        [Test]
        public void TestEvolvedPokemonEvent()
        {
            const string arg0 = "Test";
            const string arg1 = "Test2";
            const string msgTemplate = "evolved their {0} into a {1}!";
            var msg = string.Format(CultureInfo.InvariantCulture, msgTemplate, arg0, arg1);
            Assert.True(PlayerEventParser.TryParse(msg, out var @event));
            Assert.IsInstanceOf<EvolvedPokemonEvent>(@event);
            Assert.AreEqual(PlayerEventType.EvolvedPokemon, @event!.EventType);
            Assert.AreEqual(arg0, ((EvolvedPokemonEvent) @event).PokemonName);
            Assert.AreEqual(arg1, ((EvolvedPokemonEvent) @event).EvolvedPokemonName);
        }

        [Test]
        public void TestDefeatedByWildPokemonEvent()
        {
            const string arg0 = "Test";
            const string msgTemplate = "got defeated by a wild {0}.";
            var msg = string.Format(CultureInfo.InvariantCulture, msgTemplate, arg0);
            Assert.True(PlayerEventParser.TryParse(msg, out var @event));
            Assert.IsInstanceOf<DefeatedByWildPokemonEvent>(@event);
            Assert.AreEqual(PlayerEventType.DefeatedByWildPokemon, @event!.EventType);
            Assert.AreEqual(arg0, ((DefeatedByWildPokemonEvent) @event).PokemonName);
        }

        [Test]
        public void TestDefeatedByTrainerEvent()
        {
            const string arg0 = "Test Test2";
            const string msgTemplate = "got defeated by {0}.";
            var msg = string.Format(CultureInfo.InvariantCulture, msgTemplate, arg0);
            Assert.True(PlayerEventParser.TryParse(msg, out var @event));
            Assert.IsInstanceOf<DefeatedByTrainerEvent>(@event);
            Assert.AreEqual(PlayerEventType.DefeatedByTrainer, @event!.EventType);
            Assert.AreEqual(arg0, ((DefeatedByTrainerEvent) @event).TrainerTypeAndName);
        }
    }
}