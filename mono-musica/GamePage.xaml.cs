using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Input;

using MonoGame.Framework;
using Windows.ApplicationModel.Activation;
using SharpDX;
using SharpDX.XAudio2;
using SharpDX.Multimedia;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;


namespace mono_musica
{
    /// <summary>
    /// The root page used to display the game.
    /// </summary>
    public sealed partial class GamePage : SwapChainBackgroundPanel
    {
        readonly Game1 _game;
        Dictionary<uint, sonido> dictSonidos = new Dictionary<uint,sonido>();

        
        XAudio2 xaudio2;
        MasteringVoice masteringVoice;

        private sonido.onda syntActual= sonido.onda.sinusoidal ;
        private sonido.onda envoActual = sonido.onda.ninguno;


        static int envolventeMax = 60;
        private int envolventeFreq = 0;

        public GamePage(LaunchActivatedEventArgs args)
        {
            this.InitializeComponent();
            

            // Create the game.
            _game = XamlGame<Game1>.Create(args, Window.Current.CoreWindow, this);

            xaudio2 = new XAudio2();
            masteringVoice = new MasteringVoice(xaudio2);

            textbox1.Text = "";

            perilla1.RenderTransform = new CompositeTransform();
            perilla2.RenderTransform = new CompositeTransform();
            perilla3.RenderTransform = new CompositeTransform();

        }

       

        private void pointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Windows.UI.Xaml.Input.Pointer ptr = e.Pointer;
            PointerPoint ptrPt = e.GetCurrentPoint(img);
            uint pointerId = ptrPt.PointerId;

            textbox1.Text  += "P id:"+ptrPt.PointerId+"\tX:\t" + ptrPt.Position.X +"\tY:\t" + ptrPt.Position.Y+"\n";

            int frecuencia = (int)(ptrPt.Position.X * 2048.0f / 640.0f);
            float vol = (float)(ptrPt.Position.Y / 1280.0f);
            if (!dictSonidos.ContainsKey(ptrPt.PointerId))
            {
                dictSonidos.Add(ptrPt.PointerId, new sonido(vol, frecuencia, ptrPt.PointerId, xaudio2, syntActual, envoActual, envolventeFreq));
              
                dictSonidos[pointerId].play();
            }
            else
            {
                //dictSonidos[ptrPt.PointerId]= new sonido(vol,frecuencia, ptrPt.PointerId, xaudio2);
                dictSonidos[pointerId].play();
            }

            e.Handled = true;

            
            

        }

        private void pointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Windows.UI.Xaml.Input.Pointer ptr = e.Pointer;
            PointerPoint ptrPt = e.GetCurrentPoint(img);
            uint pointerId = ptrPt.PointerId;
            
            int frecuenciaN = (int)(ptrPt.Position.X * 2048.0f / 640.0f);
            float volN = (float)(ptrPt.Position.Y / 1280.0f);
            if (dictSonidos.ContainsKey(pointerId))
            {
                dictSonidos[pointerId].cambioFrec(frecuenciaN, envolventeFreq);
                dictSonidos[pointerId].cambioVol(volN);
                dictSonidos[pointerId].play();
            }
            /*
            Windows.UI.Xaml.Input.Pointer ptr = e.Pointer;
            PointerPoint ptrPt = e.GetCurrentPoint(img);
            textbox1.Text += "M:id:" + ptrPt.PointerId + "\tX:\t" + ptrPt.Position.X + "\tY:\t" + ptrPt.Position.Y + "\n";
             * */
            e.Handled = true;
        }


        private void pointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Windows.UI.Xaml.Input.Pointer ptr = e.Pointer;
            PointerPoint ptrPt = e.GetCurrentPoint(img);
            uint pointerId = ptrPt.PointerId;
            if (dictSonidos.ContainsKey(pointerId))
            {
                dictSonidos.Remove(pointerId);
            }
            e.Handled = true;
        }


        private void btnOndaPress(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement boton = (FrameworkElement)e.OriginalSource;
            switch (boton.Name)
            {
                case "swOnda1":
                    {
                        swOnda1.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_on.jpg", UriKind.Absolute) };
                        swOnda2.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swOnda3.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        syntActual = sonido.onda.sinusoidal;

                        break;
                    }
                case "swOnda2":
                    {
                        swOnda1.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swOnda2.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_on.jpg", UriKind.Absolute) };
                        swOnda3.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        syntActual = sonido.onda.triangular;
                        break;
                    }
                case "swOnda3":
                    {
                        swOnda1.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swOnda2.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swOnda3.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_on.jpg", UriKind.Absolute) };
                        syntActual = sonido.onda.cuadrada;
                        break;
                    }
            }
            e.Handled = true;
        }

        private void btnEnvolventePress(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement boton = (FrameworkElement)e.OriginalSource;
            switch (boton.Name)
            {
                case "swEnv1":
                    {
                        swEnv1.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_on.jpg", UriKind.Absolute) };
                        swEnv2.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swEnv3.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swEnv4.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        envoActual = sonido.onda.sinusoidal;

                        var transform = perilla1.RenderTransform  as CompositeTransform;
                        envolventeFreq = (int)(transform.Rotation * envolventeMax / 180);
                        textbox1.Text += "P1: " + envolventeFreq+"\n";
                        
                        break;
                    }
                case "swEnv2":
                    {
                        swEnv1.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swEnv2.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_on.jpg", UriKind.Absolute) };
                        swEnv3.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swEnv4.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        envoActual = sonido.onda.triangular;
                        var transform = perilla2.RenderTransform as CompositeTransform;
                        envolventeFreq = (int)(transform.Rotation * envolventeMax / 180);
                        textbox1.Text += "P2: " + envolventeFreq + "\n";
                        break;
                    }
                case "swEnv3":
                    {
                        swEnv1.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swEnv2.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swEnv3.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_on.jpg", UriKind.Absolute) };
                        swEnv4.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        envoActual = sonido.onda.cuadrada;
                        var transform = perilla3.RenderTransform as CompositeTransform;
                        envolventeFreq = (int)(transform.Rotation * envolventeMax / 180);
                        textbox1.Text += "P3: " + envolventeFreq + "\n";
                        break;
                    }
                case "swEnv4":
                    {
                        swEnv1.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swEnv2.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swEnv3.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_off.jpg", UriKind.Absolute) };
                        swEnv4.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/sw_on.jpg", UriKind.Absolute) };
                        envoActual = sonido.onda.ninguno;
                        break;
                    }
            }

            e.Handled = true;


        }

        private void rotacionPerilla(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            FrameworkElement source = e.OriginalSource as FrameworkElement;
            var transform = source.RenderTransform as CompositeTransform;
            transform.Rotation += e.Delta.Rotation;

            if (transform.Rotation < 0) { transform.Rotation = 0; }
            if (transform.Rotation >180 ) { transform.Rotation = 180; }
            source.RenderTransform = transform;


            if (source.Name == "perilla1" && envoActual == sonido.onda.sinusoidal)
            {
                envolventeFreq = (int)(transform.Rotation * envolventeMax / 180);
            }

            if (source.Name == "perilla2" && envoActual == sonido.onda.triangular)
            {
                envolventeFreq = (int)(transform.Rotation * envolventeMax / 180);
            }

            if (source.Name == "perilla3" && envoActual == sonido.onda.cuadrada)
            {
                envolventeFreq = (int)(transform.Rotation * envolventeMax / 180);
            }


            e.Handled = true;
            
            
        }



        
 
    }
}
