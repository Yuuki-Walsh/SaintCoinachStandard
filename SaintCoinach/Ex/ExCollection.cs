﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using SaintCoinach.IO;

using File = SaintCoinach.IO.File;
using System.Collections.Concurrent;

namespace SaintCoinach.Ex {
    public class ExCollection {
        #region Fields

        private readonly Dictionary<int, string> _SheetIdentifiers = new Dictionary<int, string>();

        private readonly ConcurrentDictionary<string, WeakReference<ISheet>> _Sheets =
            new ConcurrentDictionary<string, WeakReference<ISheet>>();

        private HashSet<string> _AvailableSheets;

        #endregion

        #region Properties

        public PackCollection PackCollection { get; private set; }
        public Language ActiveLanguage { get; set; }
        public string ActiveLanguageCode { get { return ActiveLanguage.GetCode(); } }
        public IEnumerable<string> AvailableSheets { get { return _AvailableSheets; } }

        #endregion

        #region Constructors

        #region Constructor

        public ExCollection(PackCollection packCollection) {
            PackCollection = packCollection;

            BuildIndex();
        }

        #endregion

        #endregion

        #region Index

        private void BuildIndex() {
            File exRoot = PackCollection.GetFile("exd/root.exl");

            List<string> available = new List<string>();

            using (MemoryStream ms = new MemoryStream(exRoot.GetData())) {
                using (StreamReader s = new StreamReader(ms, Encoding.ASCII)) {
                    s.ReadLine(); // EXLT,2

                    while (!s.EndOfStream) {
                        string line = s.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] split = line.Split(',');
                        if (split.Length != 2)
                            continue;

                        string name = split[0];
                        int id = int.Parse(split[1]);

                        available.Add(name);
                        if (id >= 0)
                            _SheetIdentifiers.Add(id, name);
                    }
                }
            }

            _AvailableSheets = new HashSet<string>(available);
        }

        #endregion

        #region Get

        public bool SheetExists(int id) {
            return _SheetIdentifiers.ContainsKey(id);
        }

        public bool SheetExists(string name) {
            //name = FixName(name);
            return _AvailableSheets.Contains(name);
        }

        public ISheet<T> GetSheet<T>(int id) where T : IRow {
            return (ISheet<T>)GetSheet(id);
        }

        public ISheet GetSheet(int id) {
            string name = _SheetIdentifiers[id];
            return GetSheet(name);
        }

        public ISheet<T> GetSheet<T>(string name) where T : IRow {
            return (ISheet<T>)GetSheet(name);
        }

        public ISheet GetSheet(string name) {
            const string ExHPathFormat = "exd/{0}.exh";

            if (_Sheets.TryGetValue(name, out WeakReference<ISheet> sheetRef) && sheetRef.TryGetTarget(out ISheet sheet)) return sheet;

            //name = FixName(name);
            if (!_AvailableSheets.Contains(name))
                throw new KeyNotFoundException($"Unknown sheet '{name}'");

            string exhPath = string.Format(ExHPathFormat, name);
            File exh = PackCollection.GetFile(exhPath);

            Header header = CreateHeader(name, exh);
            sheet = CreateSheet(header);

            _Sheets.GetOrAdd(name, n => new WeakReference<ISheet>(sheet)).SetTarget(sheet);
            return sheet;
        }

        public string FixName(string name) {
            string[] res = _AvailableSheets.Where(_ => string.Equals(name, _, StringComparison.OrdinalIgnoreCase)).ToArray();

            return res.Any() ? res.First() : name;
        }

        #endregion

        #region Factory

        protected virtual Header CreateHeader(string name, File file) {
            return new Header(this, name, file);
        }

        protected virtual ISheet CreateSheet(Header header) {
            if (header.Variant == 1)
                return CreateSheet<Variant1.DataRow>(header);
            return CreateSheet<Variant2.DataRow>(header);
        }

        private ISheet CreateSheet<T>(Header header) where T : IDataRow {
            if (header.AvailableLanguagesCount >= 1)
                return new MultiSheet<MultiRow, T>(this, header);
            return new DataSheet<T>(this, header, header.AvailableLanguages.First());
        }

        #endregion
    }
}
