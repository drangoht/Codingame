using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
/**
 * Score points by scanning valuable fish faster than your opponent.
 **/
class Player
{
    public const int WIDTH = 10000;
    public const int HEIGHT = 10000;

    public const int DRONES_PER_PLAYER = 2;

    public const int UGLY_UPPER_Y_LIMIT = 2500;
    public const int DRONE_UPPER_Y_LIMIT = 0;
    public const int DRONE_START_Y = 500;

    public const int COLORS_PER_FISH = 4;
    public const int DRONE_MAX_BATTERY = 30;
    public const int LIGHT_BATTERY_COST = 5;
    public const int DRONE_BATTERY_REGEN = 1;
    public const int DRONE_MAX_SCANS = int.MaxValue;

    public const int DARK_SCAN_RANGE = 800;
    public const int LIGHT_SCAN_RANGE = 2000;
    public const int UGLY_EAT_RANGE = 300;
    public const int DRONE_HIT_RANGE = 200;
    public const int FISH_HEARING_RANGE = (DARK_SCAN_RANGE + LIGHT_SCAN_RANGE) / 2;

    public const int DRONE_MOVE_SPEED = 600;
    public const int DRONE_SINK_SPEED = 300;
    public const int DRONE_EMERGENCY_SPEED = 300;
    public const double DRONE_MOVE_SPEED_LOSS_PER_SCAN = 0;

    public const int FISH_SWIM_SPEED = 200;
    public const int FISH_AVOID_RANGE = 600;
    public const int FISH_FLEE_SPEED = 400;
    public const int UGLY_ATTACK_SPEED = (int)(DRONE_MOVE_SPEED * 0.9);
    public const int UGLY_SEARCH_SPEED = (int)(UGLY_ATTACK_SPEED / 2);

    public const int FISH_X_SPAWN_LIMIT = 1000;
    public const int FISH_SPAWN_MIN_SEP = 1000;
    static void Main(string[] args)
    {
        string[] inputs;
        List<FishDetail> fishDetails = new List<FishDetail>();
        int creatureCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < creatureCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int creatureId = int.Parse(inputs[0]);
            int color = int.Parse(inputs[1]);
            int type = int.Parse(inputs[2]);
            fishDetails.Add(new(creatureId, color, type));
        }

        bool noMoreFish = false;
        Vector currentVectorDirection = new(0, 0);
        List<CurrentDirection> currentDirections = new();
        List<FollowedFish> followedFishes = new();
        List<int> goUpDrone = new();
        List<Fish> monsters = new();
        bool goDeep = true;
        int evadingTurnCount = 5;
        int evadingIncrement = 0;
        bool evading = false;
        // game loop
        while (true)
        {
            List<int> myScans = new();
            List<int> foeScans = new();
            List<Drone> droneById = new();
            List<Drone> myDrones = new();
            List<Drone> foeDrones = new();
            List<Fish> visibleFishes = new();
            List<RadarBlip> myRadarBlips = new();

            int myScore = int.Parse(Console.ReadLine());
            int foeScore = int.Parse(Console.ReadLine());
            int myScanCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < myScanCount; i++)
            {
                int creatureId = int.Parse(Console.ReadLine());
                myScans.Add(creatureId);
            }

            int foeScanCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < foeScanCount; i++)
            {
                int creatureId = int.Parse(Console.ReadLine());
                foeScans.Add(creatureId);
            }

            int myDroneCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < myDroneCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int droneId = int.Parse(inputs[0]);
                int droneX = int.Parse(inputs[1]);
                int droneY = int.Parse(inputs[2]);
                int emergency = int.Parse(inputs[3]);
                int battery = int.Parse(inputs[4]);

                Vector pos = new Vector(droneX, droneY);
                Drone drone = new(droneId, pos, new Vector(0, 0), emergency, battery, new());
                droneById.Add(drone);
                myDrones.Add(drone);
                //myRadarBlips.Add(new(droneId, string.Empty));

            }

            int foeDroneCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < foeDroneCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int droneId = int.Parse(inputs[0]);
                int droneX = int.Parse(inputs[1]);
                int droneY = int.Parse(inputs[2]);
                int emergency = int.Parse(inputs[3]);
                int battery = int.Parse(inputs[4]);
                Vector pos = new Vector(droneX, droneY);
                Drone drone = new(droneId, pos, new Vector(0, 0), emergency, battery, new());
                droneById.Add(drone);
                foeDrones.Add(drone);
            }

            int droneScanCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < droneScanCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int droneId = int.Parse(inputs[0]);
                int creatureId = int.Parse(inputs[1]);

                droneById.First(d => d.DroneId == droneId).Scans.Add(creatureId);
            }

            int visibleCreatureCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < visibleCreatureCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int creatureId = int.Parse(inputs[0]);
                int creatureX = int.Parse(inputs[1]);
                int creatureY = int.Parse(inputs[2]);
                int creatureVx = int.Parse(inputs[3]);
                int creatureVy = int.Parse(inputs[4]);
                var pos = new Vector(creatureX, creatureY);
                var speed = new Vector(creatureVx, creatureVy);
                visibleFishes.Add(new(creatureId, pos, speed, fishDetails.FirstOrDefault(f => f.FishId == creatureId)));
            }

            int radarBlipCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < radarBlipCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int droneId = int.Parse(inputs[0]);
                int creatureId = int.Parse(inputs[1]);
                string radar = inputs[2];
                myRadarBlips.Add(new(droneId, creatureId, radar));
            }
            // Get all scanned fish by my drones
            var allscansNotSaved = droneById.SelectMany(drone => drone.Scans, (drone, scans) => new { drone, scans })
                .Where(ds => myDrones.Any(md => md.DroneId == ds.drone.DroneId)).Select(ds => ds.scans);

            var initMoveY = 8750;
            Vector targetPosition = new(0, 0);
            foreach (var drone in myDrones)
            {
                double dist = 0;
                var light = 0;
                var targetX = drone.Pos.X;
                var targetY = drone.Pos.Y;

                if (evading)
                {
                    evadingIncrement--;
                    if (evadingIncrement > 0)
                    {
                        targetX = targetPosition.X;
                        targetY = targetPosition.Y;
                        Console.WriteLine($"MOVE {Convert.ToInt32(targetX)} {Convert.ToInt32(targetY)} {light} Evading..."); // MOVE <x> <y> <light (1|0)> | WAIT <light (1|0)>
                        continue;
                    }
                    else
                        evading = false;
                }
                // Get Scanned fish by current drone
                var scansNotSaved = droneById.First(d => d.DroneId == drone.DroneId).Scans;


                // Get the higher type not scanned
                var fishesToSearch = fishDetails
                    .Where(f => !allscansNotSaved.Contains(f.FishId) && f.Type != -1)
                    .OrderBy(f => f.Type).FirstOrDefault();
                var typeToSearch = 0;
                if (fishesToSearch is null) // no fish 
                    noMoreFish = true;
                else
                    typeToSearch = fishesToSearch.Type;

                // get min Y for this type of fish
                var minYpopulated = GetminYFromFishType(typeToSearch);
                if (drone.Battery > 4 && targetY > minYpopulated)
                {
                    light = 1;
                }
                else
                {
                    light = 0;
                }

                visibleFishes.ForEach(f => myScans.Add(f.FishId));

                // Looking for all available directions for this drone
                var radarDirections = myRadarBlips.Where(f =>
                    f.DroneId == drone.DroneId &&
                    !droneById.SelectMany(drone => drone.Scans, (drone, scans) => new { drone, scans })
                    .Any(droneWithScans => myDrones
                        .Any(d => d.DroneId == droneWithScans.drone.DroneId &&
                                    droneWithScans.scans == f.FishId)
                        )
                    );

                if (!radarDirections.Any()) // No more fish ? Then ... wait
                    noMoreFish = true;
                else
                    noMoreFish = false;


                // Save if noMoreFish
                if (scansNotSaved.Count() == 0)
                    goUpDrone.Remove(drone.DroneId);
                if ((scansNotSaved.Count() > 12 || noMoreFish) && !goUpDrone.Any(d => d == drone.DroneId))
                    goUpDrone.Add(drone.DroneId);

                // Narrow avalaible direction for this type
                var monsterDirections = radarDirections.Where(f => fishDetails.Any(fd => fd.FishId == f.FishId && fd.Type == -1)).ToList();
                radarDirections = radarDirections.Where(f => fishDetails.Any(fd => fd.FishId == f.FishId && fd.Type == typeToSearch)).ToList();




                // Is There a followedFish
                // Is it already scanned
                // If no: follow his vector
                // Continue next drone
                Pathfinding pf = new Pathfinding();
                var followedFish = followedFishes.FirstOrDefault(d => d.DroneId == drone.DroneId);
                if (followedFish is not null)
                {
                    if (!myScans.Contains(followedFish.Fish.FishId) && !scansNotSaved.Contains(followedFish.Fish.FishId))
                    {
                        dist = Pathfinding.VectorDistance(drone.Pos, followedFish.Fish.Pos);
                        Console.WriteLine($"MOVE {followedFish.Fish.Pos.X} {followedFish.Fish.Pos.Y} {light} Fishing1... {dist}"); // MOVE <x> <y> <light (1|0)> | WAIT <light (1|0)>
                        continue;
                    }
                }


                if (goUpDrone.Any(g => g == drone.DroneId))
                {
                    targetY = 500;
                    // Monsters ?
                    targetPosition = ManageMonsters(visibleFishes, drone, new(targetX, targetY));
                    if (targetPosition != new Vector(targetX, targetY))
                    {
                        evading = true;
                        evadingIncrement = evadingTurnCount;
                    }
                    Console.Error.WriteLine($"{targetPosition.X} {targetPosition.Y} : {targetX} {targetY}");
                    targetX = targetPosition.X;
                    targetY = targetPosition.Y;
                    Console.WriteLine($"MOVE {Convert.ToInt32(targetX)} {Convert.ToInt32(targetY)} {light} Go deep..."); // MOVE <x> <y> <light (1|0)> | WAIT <light (1|0)>
                    continue;
                }

                // Go deep for init launch at start
                if (drone.Pos.Y != initMoveY && goDeep == true)
                {
                    targetY = initMoveY;
                    targetPosition = ManageMonsters(visibleFishes, drone, new(targetX, targetY));
                    if (targetPosition != new Vector(targetX, targetY))
                    {
                        evading = true;
                        evadingIncrement = evadingTurnCount;
                    }
                    targetX = targetPosition.X;
                    targetY = targetPosition.Y;

                    Console.WriteLine($"MOVE {Convert.ToInt32(targetX)} {Convert.ToInt32(targetY)} {light} Go deep..."); // MOVE <x> <y> <light (1|0)> | WAIT <light (1|0)>
                    continue;
                }
                else
                    goDeep = false;



                // Searching for the first direction for this type and store it by drone
                CurrentDirection directionToFollow = new(drone.DroneId, -1, "", 1);
                var cd = currentDirections
                    .FirstOrDefault(cd => cd.DroneId == drone.DroneId &&
                    radarDirections.Any(rd => rd.Dir == cd.Direction
                    && rd.FishId == cd.FishId));
                if (cd is null)
                {
                    currentDirections.Remove(currentDirections.FirstOrDefault(cd => cd.DroneId == drone.DroneId));
                    Console.Error.WriteLine($"{typeToSearch} fdcount: {fishDetails.Count()} {radarDirections.Count()} ");
                    var fishWithHigherType = fishDetails.FirstOrDefault(f => f.Type == typeToSearch && radarDirections.Any(r => r.FishId == f.FishId));
                    if (fishWithHigherType is null)
                    {
                        fishDetails.Remove(fishDetails.FirstOrDefault(f => f.Type == typeToSearch));
                        Console.WriteLine($"WAIT {light} Fish deleted..."); // MOVE <x> <y> <light (1|0)> | WAIT <light (1|0)>
                        continue;
                    }
                    var directionsWithType = radarDirections.Select(f => new CurrentDirection(drone.DroneId, fishWithHigherType.FishId, f.Dir, fishWithHigherType.Type));
                    directionToFollow = directionsWithType.OrderByDescending(r => r.Type).First();
                    currentDirections.Add(directionToFollow);
                    //Console.Error.WriteLine($"1type:{directionToFollow.Type} {typeToSearch}");
                }
                else
                {
                    directionToFollow = currentDirections.First(cd => cd.Type == typeToSearch && cd.DroneId == drone.DroneId);
                    //Console.Error.WriteLine($"2type:{directionToFollow.Type} {typeToSearch}");
                }


                var vectorDir = CalculateNextTargetVector(drone.Pos, directionToFollow.Direction, directionToFollow.Type);

                targetX += vectorDir.X;
                targetY = vectorDir.Y;

                targetPosition = ManageMonsters(visibleFishes, drone, new(targetX, targetY));
                if (targetPosition != new Vector(targetX, targetY))
                {
                    evading = true;
                    evadingIncrement = evadingTurnCount;
                }
                targetX = targetPosition.X;
                targetY = targetPosition.Y;

                Console.WriteLine($"MOVE {Convert.ToInt32(targetX)} {Convert.ToInt32(targetY)} {light} Searching..."); // MOVE <x> <y> <light (1|0)> | WAIT <light (1|0)>

            }
        }

        int GetminYFromFishType(int type)
        {
            switch (type)
            {
                case 0:
                    return 2500;
                case 1:
                    return 5000;
                case 2:
                    return 7500;
                default:
                    return 7500;
            }
        }
        Vector CalculateNextTargetVector(Vector orig, string direction, int type)
        {
            int ymin = 0;
            int ymax = 0;
            int xmin = 0;
            int xmax = 10000;
            int step = 600;
            switch (type)
            {
                case 0:
                    ymin = 2500;
                    ymax = 5000;
                    break;
                case 1:
                    ymin = 5000;
                    ymax = 7500;
                    break;
                case 2:
                    ymin = 7500;
                    ymax = 10000;
                    break;
                default:
                    ymin = 0;
                    ymax = 2500;
                    break;
            }
            int x = 0;
            int y = 0;
            switch (direction)
            {
                case "TL":
                    x = orig.X > xmin ? -step : 0;
                    y = ymin + step > ymax ? ymax : ymin + step;
                    break;
                case "TR":
                    x = orig.X < xmax ? step : 0;
                    y = ymin + step > ymax ? ymax : ymin + step;
                    break;
                case "BL":
                    x = orig.X > xmin ? -step : 0;
                    y = ymax - step < ymin ? ymin : ymax - step;
                    break;
                case "BR":
                    x = orig.X < xmax ? step : 0;
                    y = ymax - step < ymin ? ymin : ymax - step;
                    break;
                case "T":
                    x = 0;
                    y = 2500;
                    break;
                case "B":
                    x = 0;
                    y = 7500;
                    break;
                default:
                    x = 0;
                    break;
            }

            return new Vector(x, y);
        }
        Vector ManageMonsters(List<Fish> visibles, Drone drone, Vector targetPosition)
        {
            Vector startPosition = drone.Pos;
            Vector goalPosition = targetPosition;

            Vector nextPosition = targetPosition;
            monsters = visibles.Where(f => f.Detail != null && f.Detail.Type == -1 && Pathfinding.VectorDistance(f.Pos, drone.Pos) < 2300).ToList();
            if (monsters.Any())
            {
                Console.Error.WriteLine($"drone:{drone.DroneId} Monsters !");
                foreach (var m in monsters)
                {
                    var col = GetCollision(drone, m);
                    Console.Error.WriteLine($"col: {col.Happened} {col.T}");
                    double collisionX = m.Pos.X + col.T * (m.Speed.X - drone.Speed.X) - drone.Pos.X;
                    double collisionY = m.Pos.Y + col.T * (m.Speed.Y - drone.Speed.Y) - drone.Pos.Y;
                    double angle = Math.Atan2(collisionY, collisionX);
                    double angleDegree = (180 / Math.PI) * angle;
                    Console.Error.WriteLine($"angleDegree: {angleDegree}");
                    nextPosition = targetPosition + GetEvadeDirectionFromAngle(angleDegree);

                }
                Console.Error.WriteLine($"npx:{nextPosition.X} npy:{nextPosition.Y}");
            }

            return nextPosition;
        }
        Vector GetEvadeDirectionFromAngle(double angle)
        {
            Vector direction = new Vector(0, 0);
            int step = DRONE_MOVE_SPEED + 600;
            if (angle > 0 && angle < 90) // bottom right
            {
                direction.X = -step;
                direction.Y = step;
            }
            else if (angle >= 90 && angle <= 180)
            {
                direction.X = step;
                direction.Y = step;
            }
            else if (angle <= 0 && angle < -90)
            {
                direction.X = -step;
                direction.Y = -step;
            }
            else if (angle <= -90 && angle <= 180)
            {
                direction.X = step;
                direction.Y = -step;
            }
            return direction;
        }
        Collision GetCollision(Drone drone, Fish ugly)
        {
            // Check instant collision
            if (ugly.Pos.InRange(drone.Pos, DRONE_HIT_RANGE + UGLY_EAT_RANGE))
            {
                return new Collision(0.0, ugly, drone);
            }

            // Both units are motionless
            if (drone.Speed.IsZero() && ugly.Speed.IsZero())
            {
                return new Collision(-1);
            }

            // Change referencial
            double x = ugly.Pos.X;
            double y = ugly.Pos.Y;
            double ux = drone.Pos.X;
            double uy = drone.Pos.Y;

            double x2 = x - ux;
            double y2 = y - uy;
            double r2 = UGLY_EAT_RANGE + DRONE_HIT_RANGE + 600;
            double vx2 = ugly.Speed.X - drone.Speed.X;
            double vy2 = ugly.Speed.Y - drone.Speed.Y;

            // Resolving: sqrt((x + t*vx)^2 + (y + t*vy)^2) = radius <=> t^2*(vx^2 + vy^2) + t*2*(x*vx + y*vy) + x^2 + y^2 - radius^2 = 0
            // at^2 + bt + c = 0;
            // a = vx^2 + vy^2
            // b = 2*(x*vx + y*vy)
            // c = x^2 + y^2 - radius^2 

            double a = vx2 * vx2 + vy2 * vy2;

            if (a <= 0.0)
            {
                return new Collision(-1);
            }

            double b = 2.0 * (x2 * vx2 + y2 * vy2);
            double c = x2 * x2 + y2 * y2 - r2 * r2;
            double delta = b * b - 4.0 * a * c;

            if (delta < 0.0)
            {
                return new Collision(-1);
            }

            double t = (-b - Math.Sqrt(delta)) / (2.0 * a);

            if (t <= 0.0)
            {
                return new Collision(-1);
            }

            if (t > 1.0)
            {
                return new Collision(-1);
            }
            return new Collision(t, ugly, drone);
        }
    }



}




public record FishDetail(int FishId, int Color, int Type);

public record Fish(int FishId, Vector Pos, Vector Speed, FishDetail Detail);

public record Drone(int DroneId, Vector Pos, Vector Speed, int Emergency, int Battery, List<int> Scans);

public record RadarBlip(int DroneId, int FishId, string Dir);

public record FollowedFish(int DroneId, Fish Fish);
public record CurrentDirection(int DroneId, int FishId, string Direction, int Type);

public class Vector
{
    public double X { get; set; }
    public double Y { get; set; }
    public Vector(double x, double y)
    {
        X = x;
        Y = y;
    }

    public bool InRange(Vector v, double range)
    {
        return (v.X - X) * (v.X - X) + (v.Y - Y) * (v.Y - Y) <= range * range;
    }
    public bool IsZero()
    {
        return X == 0 && Y == 0;
    }
    public static Vector operator -(Vector a, Vector b)
    => new Vector(a.X - b.X, a.Y - b.Y);


    public static Vector operator +(Vector a, Vector b)
    => new Vector(a.X + b.X, a.Y + b.Y);


    public static Vector operator *(Vector a, float b)
    => new Vector(a.X * b, a.Y * b);


    public static Vector operator /(Vector a, float b)
    => new Vector(a.X / b, a.Y / b);

    double height, length, breadth;

    public static bool operator ==(Vector v1, Vector v2)
    {
        if (v1 is null)
            return v2 is null;

        return v1.Equals(v2);
    }

    public static bool operator !=(Vector v1, Vector v2)
    {
        return !(v1 == v2);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        return obj is Vector v2 ? (X == v2.X &&
                               Y == v2.Y) : false;

    }

    public override int GetHashCode()
    {
        return (X, Y).GetHashCode();
    }
}

public class Pathfinding
{
    public static double VectorDistance(Vector a, Vector b)
    {
        return (double)Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
    }
}

// From Referee
public class Collision
{

    public double T;
    Fish F;
    Drone D;
    public bool Happened => T >= 0;


    public Collision(double t)
    {
        T = t;
    }

    public Collision(double t, Drone d)
    {
        T = t;
        D = d;
    }

    public Collision(double t, Fish f, Drone d)
    {
        T = t;
        F = f;
        D = d;
    }


}