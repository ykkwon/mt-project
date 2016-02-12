namespace dbApp.FFT
{
    class SpectrumAnalyzerVisualization : IVisualizationPlugin
    {
        private SpectrumAnalyser spectrumAnalyser = new SpectrumAnalyser();

        public string Name
        {
            get { return "Spectrum Analyser"; }
        }

        public object Content
        {
            get { return spectrumAnalyser; }
        }


        public void OnMaxCalculated(float min, float max)
        {
            // nothing to do
        }

        public void OnFftCalculated(NAudio.Dsp.Complex[] result)
        {/*
            counter++;
            if(counter%30 == 0)
                Console.WriteLine(counter + " OnFftCalculated -- SpectrumAnalyzerVisualization");
            */
            spectrumAnalyser.Update(result);
        }
    }
}
