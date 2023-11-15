namespace AOSharp.Pathfinding
{
    public class SNavMeshControllerSettings
    {
        public SNavMeshSettings NavMeshSettings;
        public SPathSettings PathSettings;

        public SNavMeshControllerSettings(SNavMeshSettings navMeshSettings, SPathSettings pathSettings)
        {
            NavMeshSettings = navMeshSettings;
            PathSettings = pathSettings;    
        }

        public SNavMeshControllerSettings()
        {
            NavMeshSettings = new SNavMeshSettings();
            PathSettings = new SPathSettings(); 
        }
    }

    public class SNavMeshSettings
    {
        public bool DrawNavMesh;
        public int DrawDistance;

        public SNavMeshSettings(bool drawDebug = true, int drawDistance = 100)
        {
            DrawNavMesh = drawDebug;
            DrawDistance = drawDistance;
        }
    }

    public class SPathSettings
    {
        public bool DrawPath;

        public SPathSettings(bool drawPath = true)
        {
            DrawPath = drawPath;
        }
    }
}
