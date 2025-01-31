﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Windows.Web.Http;

namespace GameManager
{
    #pragma warning disable 0649
    class LevelContent : IDisposable
    {
        public class Chunk{
            public string name;
            public float[] position;
            public string[] layers;
            public string tileset;
            public bool outside;
            public string background;
            public string[][] storyItems;
            public Texture2D[] maps;
        };
        
        public string name;
        public string description;
        public string preview;
        public string next;
        public bool startChase;
        public int startChunk;
        public string[][] storyItems;
        public Chunk[] chunks;
        public Stream previewData;
        public Dictionary<string, Stream> textures = new Dictionary<string, Stream>();
        
        public void Resolve(GraphicsDevice device)
        {
            foreach(var chunk in chunks)
            {
                if(chunk.maps != null && 0 < chunk.maps.Length) continue;
                chunk.maps = new Texture2D[chunk.layers.Length];
                for(int i=0; i<chunk.layers.Length; ++i)
                {
                    Stream stream = textures[chunk.layers[i]];
                    chunk.maps[i] = Texture2D.FromStream(device, stream);
                    stream.Flush();
                    stream.Dispose();//stream.Close();
                }
            }
        }
        
        public void Dispose()
        {
            if (previewData != null)
            {
                previewData.Flush();
                previewData.Dispose();//.Close();
            }

            foreach (var entry in textures)
            {
                entry.Value.Flush();
                entry.Value.Dispose();//.Close();
            }
        }

        public static LevelContent Read(object level, bool readMetadata=false)
        {
            if (level is string)
                return Read((string)level, readMetadata);
            else if (level is Uri)
                return Read((Uri)level, readMetadata);
            else
                throw new ArgumentException(String.Format("{0} is not a valid level identifier. Must be a string or Uri.", level));
        }
        
        public static LevelContent Read(String level, bool readMetadata = false)
        {
            return Task.Run(()=>ReadAsync(level, readMetadata)).Result;
        }
        
        public static LevelContent Read(Uri uri, bool readMetadata = false)
        {
            return Task.Run(()=>ReadAsync(uri, readMetadata)).Result;
        }
        
        public static Task<LevelContent> ReadAsync(String level, bool readMetadata = false)
        {
            return ReadAsync(new Uri("ms-appx:///Content/Levels/"+level+".zip"), readMetadata);
        }

        public static async Task<LevelContent> ReadAsync(Uri uri, bool readMetadata = false)
        {
            if (uri.Scheme.Equals("ms-appx"))
            {
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
                var asyncStream = await file.OpenSequentialReadAsync();
                using (var stream = asyncStream.AsStreamForRead())
                    return Read(stream, readMetadata);
            }
            else if (uri.Scheme.Equals("http") || uri.Scheme.Equals("https"))
            {
                var asyncStream = await new HttpClient().GetInputStreamAsync(uri);
                using (var stream = asyncStream.AsStreamForRead())
                    return Read(stream, readMetadata);
            }
            else
                throw new ArgumentException(String.Format("{0} is not an acceptable Uri. Needs to be a local file, or http/s Url.", uri));
        }
        
        public static LevelContent Read(Stream stream, bool readMetadata = false)
        {
            LevelContent content = null;

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                // Load base content metadata
                var entry = archive.GetEntry("level.json");
                using (var jsonStream = entry.Open())
                using (var reader = new StreamReader(jsonStream))
                using (var json = new JsonTextReader(reader))
                {
                    content = new JsonSerializer().Deserialize<LevelContent>(json);
                }

                MemoryStream readZipEntry(string file)
                {
                    var localEntry = archive.GetEntry(file);
                    using (var localStream = localEntry.Open())
                    {
                        var memory = new MemoryStream();
                        localStream.CopyTo(memory);
                        return memory;
                    }
                }

                // Load texture files
                if (readMetadata)
                {
                    if (content.preview != null)
                        content.previewData = readZipEntry(content.preview);
                }
                else
                {
                    foreach (var chunk in content.chunks)
                        foreach (var layer in chunk.layers)
                            content.textures[layer] = readZipEntry(layer);
                }
            }
            
            return content;
        }
    }
    #pragma warning restore 0649
}
