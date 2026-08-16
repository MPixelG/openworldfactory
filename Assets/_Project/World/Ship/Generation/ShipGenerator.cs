using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace _Project.World.Ship.Generation
{
    public class ShipGenerator
    {

        public static ShipLayout Step(ShipLayout layout)
        {
            
            //iteration of random improvement
            //TODO finish
            return layout;
        }

        private ShipEvaluationResult Evaluate(ShipLayout layout)
        {
            //returns a score of the current design + the current design
            //TODO finish 
            return new ShipEvaluationResult();
        }


        private float calculateStability(ShipLayout layout)
        {
            //the stability of the ship, (by calculating the center of mass and stuff like that)
            //TODO finish
            return 0f;
        }

        private float3 calculateCenterOfMass(ShipLayout layout)
        {
            float3 centerOfMass = float3.zero;
            int i = 0;
            foreach (var tile in layout.Tiles)
            {
                if (tile.Type == ShipTileType.Empty) continue;
                i++;
                centerOfMass += tile.Position;
            }

            return centerOfMass * i;
        }
        
    }
}