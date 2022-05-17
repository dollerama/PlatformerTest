using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{

    /// <summary>
    /// This event is triggered when the player character enters a trigger with a VictoryZone component.
    /// </summary>
    /// <typeparam name="PlayerEnteredVictoryZone"></typeparam>
    public class PlayerEnteredVictoryZone : Simulation.Event<PlayerEnteredVictoryZone>
    {
        public VictoryZone victoryZone;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            model.player.animator.SetTrigger("victory");
            model.player.controlEnabled = false;

            //respawn
            if (model.player.VictoryEvent())
            {
                //if highscore was beat we will trigger our effect and schedule respawn for when the effect has dissipated
                Camera.main.GetComponent<ParticleSystem>().Play();
                Simulation.Schedule<PlayerDeath>(6);
            }
            else
            {
                //otherwise normal respawn
                Simulation.Schedule<PlayerDeath>(2);
            }
        }
    }
}