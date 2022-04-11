using NUnit.Framework;

using P3D.Legacy.Common.Events;
using P3D.Legacy.Tests.Utils;

namespace P3D.Legacy.Tests
{
    public class EventParserTests
    {
        // achieved the emblem "{emblem}"!
        // hosted a battle: "Player {playername} got defeated by Player {trainername}".
        // evolved their {pokemonname} into a {pokemonname}!
        // got defeated by {trainertype} {trainername}.
        // got defeated by a wild {pokemonname}.

        [Test]
        public void TestAchievedEmblemEvent()
        {
            using var testService = TestService.CreateNew();

            testService.DoTest(serviceProvider =>
            {
                var arg0 = "Test";
                var msgTemplate = "achieved the emblem \"{0}\"!";
                var msg = string.Format(msgTemplate, arg0);
                Assert.True(EventParser.TryParse(msg, out var @event));
                Assert.IsInstanceOf<AchievedEmblemEvent>(@event);
                Assert.AreEqual(EventType.AchievedEmblem, @event.EventType);
                Assert.AreEqual(arg0, ((AchievedEmblemEvent) @event).Emblem);
            });
        }

        [Test]
        public void TestHostedABattleEvent()
        {
            using var testService = TestService.CreateNew();

            testService.DoTest(serviceProvider =>
            {
                var arg0 = "Test";
                var arg1 = "Test2";
                var msgTemplate = "hosted a battle: \"Player {0} got defeated by Player {1}\".";
                var msg = string.Format(msgTemplate, arg0, arg1);
                Assert.True(EventParser.TryParse(msg, out var @event));
                Assert.IsInstanceOf<HostedABattleEvent>(@event);
                Assert.AreEqual(EventType.HostedABattle, @event.EventType);
                Assert.AreEqual(arg0, ((HostedABattleEvent) @event).DefeatedTrainer);
                Assert.AreEqual(arg1, ((HostedABattleEvent) @event).Trainer);
            });
        }

        [Test]
        public void TestEvolvedPokemonEvent()
        {
            using var testService = TestService.CreateNew();

            testService.DoTest(serviceProvider =>
            {
                var arg0 = "Test";
                var arg1 = "Test2";
                var msgTemplate = "evolved their {0} into a {1}!";
                var msg = string.Format(msgTemplate, arg0, arg1);
                Assert.True(EventParser.TryParse(msg, out var @event));
                Assert.IsInstanceOf<EvolvedPokemonEvent>(@event);
                Assert.AreEqual(EventType.EvolvedPokemon, @event.EventType);
                Assert.AreEqual(arg0, ((EvolvedPokemonEvent) @event).PokemonName);
                Assert.AreEqual(arg1, ((EvolvedPokemonEvent) @event).EvolvedPokemonName);
            });
        }

        [Test]
        public void TestDefeatedByWildPokemonEvent()
        {
            using var testService = TestService.CreateNew();

            testService.DoTest(serviceProvider =>
            {
                var arg0 = "Test";
                var msgTemplate = "got defeated by a wild {0}.";
                var msg = string.Format(msgTemplate, arg0);
                Assert.True(EventParser.TryParse(msg, out var @event));
                Assert.IsInstanceOf<DefeatedByWildPokemonEvent>(@event);
                Assert.AreEqual(EventType.DefeatedByWildPokemon, @event.EventType);
                Assert.AreEqual(arg0, ((DefeatedByWildPokemonEvent) @event).PokemonName);
            });
        }

        [Test]
        public void TestDefeatedByTrainerEvent()
        {
            using var testService = TestService.CreateNew();

            testService.DoTest(serviceProvider =>
            {
                var arg0 = "Test Test2";
                var msgTemplate = "got defeated by {0}.";
                var msg = string.Format(msgTemplate, arg0);
                Assert.True(EventParser.TryParse(msg, out var @event));
                Assert.IsInstanceOf<DefeatedByTrainerEvent>(@event);
                Assert.AreEqual(EventType.DefeatedByTrainer, @event.EventType);
                Assert.AreEqual(arg0, ((DefeatedByTrainerEvent) @event).TrainerTypeAndName);
            });
        }
    }
}