using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoGame.Framework;
using Windows.ApplicationModel.Activation;
using SharpDX;
using SharpDX.XAudio2;
using SharpDX.Multimedia;
using System;
using System.IO;
using System.Collections;
using Windows.UI.Xaml.Media.Imaging;

namespace mono_musica
{
    class sonido
    {
        public enum onda
        {
            sinusoidal,
            triangular,
            cuadrada,
            ninguno
        }
        private int frec;

        private float vol = 1.0f;
        private uint pointerId;
        private SourceVoice sourcevoice;
        private static int sampleRate = 44100;
        // buffer de 1s
        private short[] BufferSamples = new short[sampleRate * 2 * 1];
        //private static short[] BufferSamples = new short[2048];

        private onda tipoOnda = onda.sinusoidal;
        private onda tipoEnvolvente = onda.ninguno;

        public sonido(float volumen, int frecuencia, uint puntero, XAudio2 xaudio, onda ondaOriginal, onda envoOriginal, int frecuenciaEnvolvente)
        {
            frec = frecuencia;
            vol = volumen / 2.0f;
            tipoOnda = ondaOriginal;
            tipoEnvolvente = envoOriginal;

            llenaBuffer(BufferSamples, frec, frecuenciaEnvolvente);
            pointerId = puntero;
            var waveFormat = new WaveFormat();


            sourcevoice = new SourceVoice(xaudio, waveFormat, true);
            /*
            FilterParameters filtro = new FilterParameters();
            filtro.Frequency = 0.3f;
            filtro.Type = 0;
            filtro.OneOverQ = 1.0f;
            sourcevoice.SetFilterParameters(filtro);
            */
        }

        public void llenaBuffer(short[] buffer, int frecuencia, int frecuenciaEnvolvente)
        {
            double totalTime;

            // generamos el buffer de la envoltura
            totalTime = 0;
            double[] bufferE = new double[buffer.Length];
            for (int i = 0; i < buffer.Length - 1; i += 2)
            {
                double time = (double)totalTime / (double)sampleRate;
                double currentSample = 0;
                if (this.tipoEnvolvente == onda.sinusoidal)
                {
                    currentSample = (double)(Math.Sin(2 * Math.PI * frecuenciaEnvolvente * time));
                }

                if (this.tipoEnvolvente == onda.triangular)
                {
                    currentSample = (double)(2 * (time * frecuenciaEnvolvente - Math.Floor(time * frecuenciaEnvolvente + 0.5)));
                }

                if (this.tipoEnvolvente == onda.cuadrada)
                {
                    currentSample = (double)(Math.Sin(2 * Math.PI * frecuenciaEnvolvente * time) >= 0 ? 1 : 0);

                }

                if (this.tipoEnvolvente == onda.ninguno)
                {
                    currentSample = 1;
                }


                bufferE[i] = currentSample;
                bufferE[i + 1] = currentSample;

                totalTime++;
            }

            // generamos la onda
            totalTime = 0;
            for (int i = 0; i < buffer.Length - 1; i += 2)
            {
                double time = (double)totalTime / (double)sampleRate;
                short currentSample = 0;

                if (this.tipoOnda == onda.sinusoidal)
                {
                    currentSample = (short)(Math.Sin(2 * Math.PI * frecuencia * time) * (double)short.MaxValue);
                }

                if (this.tipoOnda == onda.triangular)
                {
                    currentSample = (short)(2 * (time * frecuencia - Math.Floor(time * frecuencia + 0.5)) * (double)short.MaxValue);
                }

                if (this.tipoOnda == onda.cuadrada)
                {
                    currentSample = (short)(Math.Sin(2 * Math.PI * frecuencia * time) >= 0 ? short.MaxValue : short.MinValue);

                }


                buffer[i] = (short)(currentSample * bufferE[i]);
                buffer[i + 1] = (short)(currentSample * bufferE[i + 1]);

                totalTime++;
            }
        }

        public void play()
        {
            var dataStream = DataStream.Create(BufferSamples, true, true);
            AudioBuffer buffer = new AudioBuffer
            {
                /*LoopCount = AudioBuffer.LoopInfinite,*/
                Stream = dataStream,
                AudioBytes = (int)dataStream.Length,
                Flags = BufferFlags.EndOfStream
            };


            sourcevoice.SubmitSourceBuffer(buffer, null);
            sourcevoice.SetVolume(vol);




            sourcevoice.Start();



        }

        public void cambioFrec(int frecuenciaNueva, int frecuenciaEnvolvente)
        {
            if (sourcevoice != null)
            {

                sourcevoice.SetVolume(0.0f);
                sourcevoice.FlushSourceBuffers();

                //sourcevoice.SetFrequencyRatio(frecuenciaNueva / 2048);


            }
            sourcevoice.SetVolume(vol);
            llenaBuffer(BufferSamples, frecuenciaNueva, frecuenciaEnvolvente);
            sourcevoice.Start();

        }

        public void cambioOnda(onda ondaNueva)
        {
            this.tipoOnda = ondaNueva;
        }

        public void cambioVol(float volumen)
        {
            if (sourcevoice != null)
            {
                vol = volumen / 2.0f;
                sourcevoice.SetVolume(vol);

            }
        }



    }
}
