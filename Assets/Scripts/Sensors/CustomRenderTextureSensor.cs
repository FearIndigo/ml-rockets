using System;
using FearIndigo.Utility;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace FearIndigo.Sensors
{
    /// <summary>
    /// Sensor class that wraps a [RenderTexture](https://docs.unity3d.com/ScriptReference/RenderTexture.html) instance.
    /// </summary>
    public class CustomRenderTextureSensor : ISensor, IDisposable
    {
        RenderTexture m_RenderTexture;
        bool m_Grayscale;
        string m_Name;
        private ObservationSpec m_ObservationSpec;
        SensorCompressionType m_CompressionType;
        Texture2D m_Texture;
        public event Action OnUpdate;

        /// <summary>
        /// The compression type used by the sensor.
        /// </summary>
        public SensorCompressionType CompressionType
        {
            get { return m_CompressionType; }
            set { m_CompressionType = value; }
        }


        /// <summary>
        /// Initializes the sensor.
        /// </summary>
        /// <param name="renderTexture">The [RenderTexture](https://docs.unity3d.com/ScriptReference/RenderTexture.html)
        /// instance to wrap.</param>
        /// <param name="grayscale">Whether to convert it to grayscale or not.</param>
        /// <param name="name">Name of the sensor.</param>
        /// <param name="compressionType">Compression method for the render texture.</param>
        /// <param name="preUpdate"></param>
        /// [GameObject]: https://docs.unity3d.com/Manual/GameObjects.html
        public CustomRenderTextureSensor(
            RenderTexture renderTexture, bool grayscale, string name, SensorCompressionType compressionType)
        {
            m_RenderTexture = renderTexture;
            var width = renderTexture != null ? renderTexture.width : 0;
            var height = renderTexture != null ? renderTexture.height : 0;
            m_Grayscale = grayscale;
            m_Name = name;
            m_ObservationSpec = ObservationSpec.Visual(grayscale ? 1 : 3, height, width);
            m_CompressionType = compressionType;
            m_Texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        }

        /// <inheritdoc/>
        public string GetName()
        {
            return m_Name;
        }

        /// <inheritdoc/>
        public ObservationSpec GetObservationSpec()
        {
            return m_ObservationSpec;
        }

        /// <inheritdoc/>
        public byte[] GetCompressedObservation()
        {
            ObservationToTexture(m_RenderTexture, m_Texture);
            // TODO support more types here, e.g. JPG
            var compressed = m_Texture.EncodeToPNG();
            return compressed;
        }

        /// <inheritdoc/>
        public int Write(ObservationWriter writer)
        {
            ObservationToTexture(m_RenderTexture, m_Texture);
            var numWritten = writer.WriteTexture(m_Texture, m_Grayscale);
            return numWritten;
        }

        /// <inheritdoc/>
        public void Update()
        {
            OnUpdate?.Invoke();
        }

        /// <inheritdoc/>
        public void Reset() { }

        /// <inheritdoc/>
        public CompressionSpec GetCompressionSpec()
        {
            return new CompressionSpec(m_CompressionType);
        }

        /// <summary>
        /// Converts a RenderTexture to a 2D texture.
        /// </summary>
        /// <param name="obsTexture">RenderTexture.</param>
        /// <param name="texture2D">Texture2D to render to.</param>
        public static void ObservationToTexture(RenderTexture obsTexture, Texture2D texture2D)
        {
            var prevActiveRt = RenderTexture.active;
            RenderTexture.active = obsTexture;

            texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = prevActiveRt;
        }

        /// <summary>
        /// Clean up the owned Texture2D.
        /// </summary>
        public void Dispose()
        {
            if (!ReferenceEquals(null, m_Texture))
            {
                TextureHelper.DestroyTexture(m_Texture);
                m_Texture = null;
            }
        }
    }
}
