using SaintCoinach.Ex.Relational;
using SaintCoinach.Imaging;
using SaintCoinach.IO;

namespace SaintCoinach.Xiv {
    public class ClassJob : XivRow {
        #region Static

        private const int IconOffset = 62000;
        private const int FramedIconOffset = 62100;
        private const string IconFormat = "ui/icon/{0:D3}000/{1:D6}.tex";

        #endregion

        #region Properties

        public Text.XivString Name { get { return AsString("Name"); } }
        public Text.XivString Abbreviation { get { return AsString("Abbreviation"); } }
        public ClassJobCategory ClassJobCategory { get { return As<ClassJobCategory>(); } }
        public ClassJob ParentClassJob { get { return As<ClassJob>("ClassJob{Parent}"); } }
        public Item StartingWeapon { get { return As<Item>("Item{StartingWeapon}"); } }
        public Item SoulCrystal { get { return As<Item>("Item{SoulCrystal}"); } }
        public byte StartingLevel {  get { return As<byte>("StartingLevel"); } }

        public ImageFile Icon {
            get {
                int nr = IconOffset + Key;
                string path = string.Format(IconFormat, nr / 1000, nr);
                if (Sheet.Collection.PackCollection.TryGetFile(path, out File file))
                    return file as ImageFile;
                return null;
            }
        }

        public ImageFile FramedIcon {
            get {
                int nr = FramedIconOffset + Key;
                string path = string.Format(IconFormat, nr / 1000, nr);
                if (Sheet.Collection.PackCollection.TryGetFile(path, out File file))
                    return file as ImageFile;
                return null;
            }
        }


        #endregion

        #region Constructors

        #region Constructor

        public ClassJob(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        #endregion

        public override string ToString() {
            return Name;
        }
    }
}
