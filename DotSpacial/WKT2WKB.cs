using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;

namespace DotSpacialTest
{
    public class WKT2WKB
    {

        public static byte[] GetWKB(string wkt)
        {
            /* -------------------------------------------------------------------- */
            /*      Register format(s).                                             */
            /* -------------------------------------------------------------------- */
            Ogr.RegisterAll();

            Geometry geom = Geometry.CreateFromWkt(wkt);
            byte[] wkb = null;

            int wkbSize = geom.WkbSize();
            if (wkbSize > 0)
            {
                wkb = new byte[wkbSize];
                geom.ExportToWkb(wkb);

                // wkb --> wkt (reverse test)
                Geometry geom2 = Geometry.CreateFromWkb(wkb);
                string geom_wkt;
                geom2.ExportToWkt(out geom_wkt);
            }

            return wkb;
        }
    }
}
