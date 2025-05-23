// BratalianData.cs
namespace Bratalian
{
    /// <summary>
    /// POCO que representa um registo em bratalian_db.json
    /// </summary>
    public class BratalianData
    {
        public int id { get; set; }
        public string name { get; set; }
        public string spriteAsset { get; set; }
        public string[] types { get; set; }
        public int baseHP { get; set; }
        public int baseAttack { get; set; }
        public int baseDefense { get; set; }
        public int baseSpeed { get; set; }
    }
}
