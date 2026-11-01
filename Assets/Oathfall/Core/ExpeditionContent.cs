namespace Oathfall.Core
{
    public enum EncounterKind { Skirmish, Ward, Sanctuary, Guardian }
    public enum EnemyRole { Raider, Hexblade, Breaker, Guardian }
    public sealed class ExpeditionStop
    {
        public readonly string Id,Name;public readonly float X,Z,WardSeconds;public readonly int Waves,Count;public readonly EncounterKind Kind;
        public ExpeditionStop(string id,string name,float x,float z,EncounterKind kind,int waves,int count,float wardSeconds=0)
        {Id=id;Name=name;X=x;Z=z;Kind=kind;Waves=waves;Count=count;WardSeconds=wardSeconds;}
    }
    public static class ExpeditionContent
    {
        public static readonly ExpeditionStop[] Stops={
            new ExpeditionStop("hollowwood.approach","Forest Approach",0,-28,EncounterKind.Skirmish,2,4),
            new ExpeditionStop("hollowwood.camp","Abandoned Camp",12,0,EncounterKind.Skirmish,3,5),
            new ExpeditionStop("hollowwood.oath","Fallen Oath Shrine",0,29,EncounterKind.Ward,0,4,75),
            new ExpeditionStop("hollowwood.quarry","Mossfall Quarry",-22,78,EncounterKind.Skirmish,3,5),
            new ExpeditionStop("hollowwood.watch","Broken Watch",20,125,EncounterKind.Skirmish,3,5),
            new ExpeditionStop("hollowwood.grove","Whispering Grove",-25,174,EncounterKind.Ward,0,5,90),
            new ExpeditionStop("hollowwood.crossing","Pilgrim Crossing",18,221,EncounterKind.Sanctuary,0,0),
            new ExpeditionStop("hollowwood.caravan","Lost Caravan",-20,268,EncounterKind.Skirmish,4,5),
            new ExpeditionStop("hollowwood.bell","Silent Bell Court",22,315,EncounterKind.Ward,0,5,105),
            new ExpeditionStop("hollowwood.gate","Oathkeeper Gate",-24,362,EncounterKind.Skirmish,4,6),
            new ExpeditionStop("hollowwood.vigil","Last Vigil",18,409,EncounterKind.Sanctuary,0,0),
            new ExpeditionStop("hollowwood.guardian","Crown of Roots",0,456,EncounterKind.Guardian,1,1)
        };
    }
    public sealed class WardProgress
    {
        public float Elapsed{get;private set;}
        public readonly float Required;
        public WardProgress(float required){Required=System.Math.Max(0,required);}
        public bool Complete=>Elapsed>=Required;
        public void Tick(float delta,bool inside,bool alive){if(inside&&alive&&delta>0)Elapsed=System.Math.Min(Required,Elapsed+delta);}
    }
}
