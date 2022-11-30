using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#if ENABLE_WINMD_SUPPORT
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
#endif

public static class Parser 
{
    public static string Base64ToJson(string base64)
    {
        Regex.Replace(base64, @"/\=+$/", "");
        Regex.Replace(base64, @"/\//g", "_");
        Regex.Replace(base64, @"/\+/g", "-");
        return base64;
    }

    //ex if string: 2020-08-14T15:54:04+01:00
    public static DateTime StringToDateTime(string datetime)
    {
        string date = Regex.Match(datetime, @"^\d{4}-\d{2}-\d{2}").ToString();
        string time = Regex.Match(datetime, @"\d{2}:\d{2}:\d{2}").ToString();

        return DateTime.Parse(string.Format("{0} {1}", date, time));

    }


#if ENABLE_WINMD_SUPPORT
    public static async Task<byte[]> ToByteArray(SoftwareBitmap sftBitmap_c)
    {
        SoftwareBitmap sftBitmap = SoftwareBitmap.Convert(sftBitmap_c, BitmapPixelFormat.Bgra8);
        InMemoryRandomAccessStream mss = new InMemoryRandomAccessStream();
        Windows.Graphics.Imaging.BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, mss);

        encoder.SetSoftwareBitmap(sftBitmap);
        await encoder.FlushAsync();

        IBuffer bufferr = new Windows.Storage.Streams.Buffer((uint)mss.Size);
        await mss.ReadAsync(bufferr, (uint)mss.Size, InputStreamOptions.None);

        DataReader dataReader = DataReader.FromBuffer(bufferr);
        byte[] bytes = new byte[bufferr.Length];
        dataReader.ReadBytes(bytes);
        return bytes;
    }
#endif
}
