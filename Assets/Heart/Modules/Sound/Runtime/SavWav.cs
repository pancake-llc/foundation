//	Copyright (c) 2012 Calvin Rien
//        http://the.darktable.com
//
//	This software is provided 'as-is', without any express or implied warranty. In
//	no event will the authors be held liable for any damages arising from the use
//	of this software.
//
//	Permission is granted to anyone to use this software for any purpose,
//	including commercial applications, and to alter it and redistribute it freely,
//	subject to the following restrictions:
//
//	1. The origin of this software must not be misrepresented; you must not claim
//	that you wrote the original software. If you use this software in a product,
//	an acknowledgment in the product documentation would be appreciated but is not
//	required.
//
//	2. Altered source versions must be plainly marked as such, and must not be
//	misrepresented as being the original software.
//
//	3. This notice may not be removed or altered from any source distribution.
//
//  =============================================================================
//
//  derived from Gregorio Zanon's script
//  http://forum.unity3d.com/threads/119295-Writing-AudioListener.GetOutputData-to-wav-problem?p=806734&viewfull=1#post806734

//
// This program is an altered version of the original code.
// 1. Changes the input parameter to [filepath] instead of [filename] in Save() method ,and comment out the filename verification and path combination code.
// 2. Fix sample length doesn't take channel into account when using AudioClip.GetData(); 
//

namespace Pancake.Sound
{
    using System;
    using System.IO;
    using UnityEngine;
    using System.Collections.Generic;

    public static class SavWav
    {
        private const int HEADER_SIZE = 44;

        public static bool Save(string filepath, AudioClip clip)
        {
            // Make sure directory exists if user is saving to sub dir.
            Directory.CreateDirectory(Path.GetDirectoryName(filepath) ?? string.Empty);

            using (var fileStream = CreateEmpty(filepath))
            {
                ConvertAndWrite(fileStream, clip);

                WriteHeader(fileStream, clip);
            }

            return true;
        }

        public static AudioClip TrimSilence(AudioClip clip, float min)
        {
            var samples = new float[clip.samples * clip.channels];

            clip.GetData(samples, 0);

            return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
        }

        public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz)
        {
            return TrimSilence(samples,
                min,
                channels,
                hz,
                false,
                false);
        }

        public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool _3D, bool stream)
        {
            int i;

            for (i = 0; i < samples.Count; i++)
            {
                if (Mathf.Abs(samples[i]) > min) break;
            }

            samples.RemoveRange(0, i);

            for (i = samples.Count - 1; i > 0; i--)
            {
                if (Mathf.Abs(samples[i]) > min) break;
            }

            samples.RemoveRange(i, samples.Count - i);
#pragma warning disable CS0618
            var clip = AudioClip.Create("TempClip",
                samples.Count / channels,
                channels,
                hz,
                _3D,
                stream);
#pragma warning restore

            clip.SetData(samples.ToArray(), 0);

            return clip;
        }

        static FileStream CreateEmpty(string filepath)
        {
            var fileStream = new FileStream(filepath, FileMode.Create);
            byte emptyByte = new byte();

            for (int i = 0; i < HEADER_SIZE; i++) //preparing the header
            {
                fileStream.WriteByte(emptyByte);
            }

            return fileStream;
        }

        static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
        {
            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            short[] intData = new short[samples.Length];
            //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

            byte[] bytesData = new byte[samples.Length * 2];
            //bytesData array is twice the size of
            //dataSource array because a float converted in Int16 is 2 bytes.

            int rescaleFactor = 32767; //to convert float to Int16

            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short) (samples[i] * rescaleFactor);
                byte[] byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            fileStream.Write(bytesData, 0, bytesData.Length);
        }

        static void WriteHeader(FileStream fileStream, AudioClip clip)
        {
            int hz = clip.frequency;
            int channels = clip.channels;
            int samples = clip.samples;

            fileStream.Seek(0, SeekOrigin.Begin);

            byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            fileStream.Write(riff, 0, 4);

            byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
            fileStream.Write(chunkSize, 0, 4);

            byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            fileStream.Write(wave, 0, 4);

            byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            fileStream.Write(fmt, 0, 4);

            byte[] subChunk1 = BitConverter.GetBytes(16);
            fileStream.Write(subChunk1, 0, 4);
#pragma warning disable CS0219
            ushort two = 2;
#pragma warning restore
            ushort one = 1;

            byte[] audioFormat = BitConverter.GetBytes(one);
            fileStream.Write(audioFormat, 0, 2);

            byte[] numChannels = BitConverter.GetBytes(channels);
            fileStream.Write(numChannels, 0, 2);

            byte[] sampleRate = BitConverter.GetBytes(hz);
            fileStream.Write(sampleRate, 0, 4);

            byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
            fileStream.Write(byteRate, 0, 4);

            ushort blockAlign = (ushort) (channels * 2);
            fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

            ushort bps = 16;
            byte[] bitsPerSample = BitConverter.GetBytes(bps);
            fileStream.Write(bitsPerSample, 0, 2);

            byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
            fileStream.Write(datastring, 0, 4);

            byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
            fileStream.Write(subChunk2, 0, 4);

            //		fileStream.Close();
        }
    }
}