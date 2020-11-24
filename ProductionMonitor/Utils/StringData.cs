namespace ProductionMonitor.Utils
{
    /// <summary>
    /// A class used to store strings.
    /// </summary>
    public static class StringData
    {
        public static readonly string ScalaReadConnStr = @"data source=Scala1;
                                                           initial catalog = Fusetech; 
                                                           user id = scala_read; 
                                                           password=scala_read;";

        public static readonly string ScalaWriteConnStr = @"data source=Scala1;
                                                            initial catalog = Fusetech; 
                                                            user id = TERMELESMONITOR; 
                                                            password=TERM123;";

        public static readonly string[] Group1 = new string[]
        {
            "BRAZING",
            "HH",
            "MV",
            "GCDAN",
            "NH-SPEC",
            "ÁRAMVÁLTÓ",
            "LMM4"
            //linocour
        };

        public static readonly string[] Group2 = new string[]
        {
            "BRAZING",
            "HH",
            "MV",
            "GCDAN",
            "NH-SPEC",
            "ÁRAMVÁLTÓ",
            "LMM4"
        };

        public static readonly string[] Group3 = new string[]
        {
            "HAL2",
            "HAL3",
            "HALM",
            "TSA",
            "NH-WEBER"
        };

        public static readonly string[] Group4 = new string[]
        {
            "LM1",
            "LM2",
            "LM3",
            "LMA000",
            "LMM",
            "LMMHC",
            "NH000W"
        };

        public static readonly string[] Munkahelyek = new string[]
        {
            "GCDAN",
            "HAL1",
            "HAL2",
            "HAL3",
            "HALM",
            "HH",
            "HH MV EXTRA",
            "LM1",
            "LM2",
            "LM3",
            "LMA000",
            "LMM",
            "LMM000",
            "LMM4",
            "LMMHC",
            "MSM3",
            "MV",
            "NH-MERSEN EXTRA",
            "NH-MSCH EXTRA",
            "NH-SPEC",
            "NH-WEBER",
            "NH-WEBER EXTRA",
            "OLVADÓSZÁL",
            "SAJTOLÓ",
            "SPEC EXTRA",
            "TSA",
            "ÁRAMVÁLTÓ",
            "ÁRAMVÁLTÓ EXTRA",
            "WASTE",
            "REWORK",
            "TROGGER"
        };
    }
}