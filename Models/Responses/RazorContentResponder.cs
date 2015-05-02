﻿using FamilyTree.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Responses
{
	public class RazorContentResponder : IContentResponder
	{
		private readonly TreeFactory _treeFactory;
		private readonly Func<string, string> _mapPath;
		private readonly TreeViewModelFactory _viewModelFactory;

		public RazorContentResponder(
			Func<string, string> mapPath,
			TreeFactory treeFactory = null,
			TreeViewModelFactory viewModelFactory = null)
		{
			_mapPath = mapPath;
			_treeFactory = treeFactory ?? new TreeFactory(mapPath);
			_viewModelFactory = viewModelFactory ?? new TreeViewModelFactory(_treeFactory, new TreeParser());
		}

		public ActionResult GetResponse(string fileName, HttpContextBase context)
		{
			var tree = _treeFactory.LoadFromFileName(fileName);
			var viewModel = _viewModelFactory.Create(tree);

			return new ViewResult
			{
				ViewData =
				{
					Model = viewModel
				}
			};
		}

		public string GetEtag(string fileName)
		{
			var xslFile = new FileInfo(_mapPath("~/Views/Family.cshtml"));
			var xslFileDateString = xslFile.LastWriteTimeUtc.ToString("yyyy-MM-dd@HH:mm:ss");

			return ETagHelper.GetEtagFromFile(new FileInfo(fileName), customEtagSuffix: xslFileDateString, includeAssemblyDate: true);
		}
	}
}