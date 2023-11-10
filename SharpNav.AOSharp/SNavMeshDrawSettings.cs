namespace AOSharp.Pathfinding
{
    public class SNavMeshControllerSettings
    {
        public SNavMeshSettings NavMeshSettings = new SNavMeshSettings();
        public SPathSettings PathSettings = new SPathSettings();
    }

    public class SNavMeshSettings
    {
        public bool DrawNavMesh = false;
        public int DrawDistance = 100;
    }

    public class SPathSettings
    {
        public bool DrawPath = false;
    }
}
