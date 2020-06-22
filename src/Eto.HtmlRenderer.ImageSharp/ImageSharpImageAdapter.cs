﻿// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"
using Eto.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters;
using System.Collections.Generic;
using System;
using System.Linq;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using SixLabors.ImageSharp.Formats.Png;
using System.Diagnostics;
using TheArtOfDev.HtmlRenderer.Eto.Adapters;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Formats.Gif;

[assembly: Eto.ExportHandler(typeof(IImageAdapter), typeof(ImageSharpImageAdapter))]

namespace TheArtOfDev.HtmlRenderer.Eto.Adapters
{
    class ImageSharpImageAdapter : RImage, IImageAdapter
    {
        IList<RImageFrame> _frames;
        int _currentFrame;
        Dictionary<int, Bitmap> _images = new Dictionary<int, Bitmap>();



        public bool Preload { get; set; }

        public SixLabors.ImageSharp.Image Image { get; private set; }

        private Bitmap GetFrame(int currentFrame)
        {
            if (_images.TryGetValue(currentFrame, out var bitmap))
                return bitmap;

            // TODO: Add frame support in Eto.Drawing.Bitmap, this can be slow and
            // it would be nice to have fewer dependencies.
            var frame = Image.Frames.CloneFrame(currentFrame);
            using (var stream = new MemoryStream())
            {
                var encoder = new PngEncoder();
                frame.Save(stream, encoder);
                stream.Position = 0;
                bitmap = new Bitmap(stream);
                _images.Add(currentFrame, bitmap);
                return bitmap;
            }
        }

        public override double Width => Image.Width;

        public override double Height => Image.Height;

        public override IList<RImageFrame> Frames => _frames ?? (_frames = GetFrames());

        private IList<RImageFrame> GetFrames()
        {
            return Image
                .Frames
                .Select(r => (RImageFrame)new ImageFrameAdapter(TimeSpan.FromMilliseconds(GetDelay(r) * 10)))
                .ToList();
        }

        public override void SetActiveFrame(int frame)
        {
            _currentFrame = frame;
        }

        int GetDelay(SixLabors.ImageSharp.ImageFrame frame)
        {
            var gifMetaData = frame.Metadata.GetFormatMetadata(GifFormat.Instance);
            return gifMetaData?.FrameDelay ?? 0;
        }

        Image IImageAdapter.Image => GetFrame(_currentFrame);

        public override void Dispose() => Image.Dispose();

        public bool Load(Stream stream)
        {
            var img = SixLabors.ImageSharp.Image.Load(stream, out var format);
            if (img.Frames.Count > 1 && img.Frames.Any(r => GetDelay(r) > 0))
            {
                Image = img;
                for (int i = 0; i < Frames.Count; i++)
                {
                    GetFrame(i);
                }
                return true;
            }
            img.Dispose();
            return false;

        }
    }
}