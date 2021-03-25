/*
   Copyright 2020 George Lasry
   Converted in 2020 from Java to C# by Nils Kopal

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace EnigmaAnalyzerLib
{
    public enum MRingScope
    {
        ALL,
        ALL_STEPPING_INSIDE_MSG,
        ALL_NON_STEPPING,
        ALL_STEPPING_INSIDE_MSG_AND_ONE_NON_STEPPING,
        STEPPING_INSIDE_MSG_WITH_SMALL_IMPACT_AND_ONE_NON_STEPPING,
        ONE_NON_STEPPING
    }

    public static class MRingScopeParser
    {
        public static MRingScope parse(int i)
        {
            switch (i)
            {
                case 0:
                    return MRingScope.ALL;
                case 1:
                    return MRingScope.ALL_STEPPING_INSIDE_MSG;
                case 2:
                    return MRingScope.ALL_NON_STEPPING;
                case 3:
                    return MRingScope.ALL_STEPPING_INSIDE_MSG_AND_ONE_NON_STEPPING;
                case 4:
                    return MRingScope.STEPPING_INSIDE_MSG_WITH_SMALL_IMPACT_AND_ONE_NON_STEPPING;
                case 5:
                    return MRingScope.ONE_NON_STEPPING;
                default:
                    return MRingScope.ALL;
            }
        }
    }
}