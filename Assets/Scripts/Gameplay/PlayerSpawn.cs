using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player is spawned after dying.
    /// </summary>
    public class PlayerSpawn : Simulation.Event<PlayerSpawn>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            model.player.ExecuteEvent();
            model.virtualCamera.m_Follow = model.player.transform;
            model.virtualCamera.m_LookAt = model.player.transform;
            Simulation.Schedule<EnablePlayerInput>(2f);
        }
    }
}