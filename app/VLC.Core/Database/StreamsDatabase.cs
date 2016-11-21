﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using VLC.Model.Stream;
using VLC.Utils;

namespace VLC.Database
{
    public class StreamsDatabase : IDatabase
    {
        private static readonly string DbPath = Strings.VideoDatabase;

        public StreamsDatabase()
        {
            Task.Run(() => Initialize());
        }

        public void Initialize()
        {
            var db = new SQLiteConnection(DbPath);
            db.CreateTable<StreamMedia>();
        }

        public void Drop()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DropTable<StreamMedia>();
            }
        }

        public void DeleteAll()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DeleteAll<StreamMedia>();
            }
        }

        public async Task<List<StreamMedia>> Load()
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return await connection.Table<StreamMedia>().ToListAsync();
        }

        public async Task Insert(StreamMedia stream)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            await connection.InsertAsync(stream);
        }

        public Task Update(StreamMedia stream)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.UpdateAsync(stream);
        }

        public Task<StreamMedia> Get(string media)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.FindAsync<StreamMedia>(x => x.Path == media);
        }

        public Task Delete(StreamMedia stream)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.DeleteAsync(stream);
        }

        public async Task<bool> Contains(StreamMedia stream)
        {
            return await Get(stream.Path) != null;
        }
    }
}
