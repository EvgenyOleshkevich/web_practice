﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_practice.Models.DB;

namespace Web_practice.Models.Pages.Task
{
	public class ResultModel
	{
		public ResultData Result { get; set; }
		public string ResultContent { get; set; }
	}
}
