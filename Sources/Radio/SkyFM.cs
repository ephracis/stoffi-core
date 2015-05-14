/***
 * SkyFM.cs
 * 
 * This file contains code for fetching radio station from SKY.fm.
 *	
 * * * * * * * * *
 * 
 * Copyright 2014 Simplare
 * 
 * This code is part of the Stoffi Music Player Project.
 * Visit our website at: stoffiplayer.com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version
 * 3 of the License, or (at your option) any later version.
 * 
 * See stoffiplayer.com/license for more information.
 ***/

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using Stoffi.Core.Media;

namespace Stoffi.Core.Sources.Radio
{
	/// <summary>
	/// SKY.fm music source for radio stations.
	/// </summary>
	public class SkyFM : DigitallyImported
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Stoffi.Core.Sources.SkyFM"/> class.
		/// </summary>
		public SkyFM()
		{
			name = "SKY.fm";
			genre = "";
			domain = "sky.fm";
			folder = "public3";
		}
	}
}