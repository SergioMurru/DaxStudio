﻿using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ADOTabular;
using Caliburn.Micro;
using DaxStudio.UI.Events;
using GongSolutions.Wpf.DragDrop;
using Serilog;
using DaxStudio.UI.Model;
using System.Collections.Generic;
using System;

namespace DaxStudio.UI.ViewModels
{
    [Export]
    public class MetadataPaneViewModel:ToolPaneBaseViewModel, IDragSource
    {
        private string _modelName;
        
        public MetadataPaneViewModel(ADOTabularConnection connection, IEventAggregator eventAggregator):base(connection,eventAggregator)
        {  }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "ModelList":
                    if (ModelList.Count > 0)
                    {
                        // if connected server is MD then reconnect with CUBE=ModelName on conn string.
                        SelectedModel = ModelList.First(m => m.Name == Connection.Database.Models.BaseModel.Name);
                    }
                    Log.Debug("{Class} {Event} {Value}", "MetadataPaneViewModel", "OnPropertyChanged:ModelList.Count", Connection.Database.Models.Count);          
                    break;
            }
        }

        public string ModelName {get {return _modelName; }
            set
            {
                if (value == _modelName)
                    return;
                _modelName = value;
               NotifyOfPropertyChange(() => ModelName);
            }
        }

        private ADOTabularModel _selectedModel;

        public ADOTabularModel SelectedModel {
            get { return _selectedModel; } 
            set {
                if (_selectedModel != value)
                {
                    _selectedModel = value;
                    _treeViewTables = null; 
                    
                    NotifyOfPropertyChange(() => SelectedModel);
                    NotifyOfPropertyChange(() => Tables);
                }
            }
        }

        public string SelectedModelName
        {
            get
            {
                return SelectedModel == null ? "--":SelectedModel.Name; 
            }
        }
        
        protected override void OnConnectionChanged()//bool isSameServer)
        {
            base.OnConnectionChanged();//isSameServer);
            if (Connection == null)
                return;
            if (ModelList == Connection.Database.Models)
                return;
            
            var ml = Connection.Database.Models;
            Log.Debug("{Class} {Event} {Value}", "MetadataPaneViewModel", "ConnectionChanged (Database)", Connection.Database.Name);          
            if (Dispatcher.CurrentDispatcher.CheckAccess())
            {
                Dispatcher.CurrentDispatcher.Invoke(new System.Action(()=> ModelList = ml));
            }
            else
            {
                ModelList = ml;
            }
            NotifyOfPropertyChange(() => IsConnected);
            NotifyOfPropertyChange(() => Connection);
        }

        //public ADOTabularTableCollection Tables {
        private IEnumerable<FilterableTreeViewItem> _treeViewTables;
        public IEnumerable<FilterableTreeViewItem> Tables {
            get
            {
                if (SelectedModel == null) return null;
                if (_treeViewTables == null)
                {
                    try
                    {
                        _treeViewTables = SelectedModel.TreeViewTables();
                    }
                    catch (Exception ex)
                    {
                        EventAggregator.PublishOnUIThread(new OutputMessage(Events.MessageType.Error,ex.Message));
                    }
                }
                return _treeViewTables;
                //return SelectedModel == null ? null : SelectedModel.TreeViewTables();
                
            }
        }

        public override string DefaultDockingPane
        {
            get { return "DockLeft"; }
            set { base.DefaultDockingPane = value; }
        }
        public override string  Title
        {
	          get { return "Metadata"; }
	          set { base.Title = value; }
        }
        
        private ADOTabularModelCollection _modelList;
        public ADOTabularModelCollection ModelList
        {
            get { return _modelList; }
            set
            {
                if (value == _modelList)
                    return;
                _modelList = value;
                NotifyOfPropertyChange(() => ModelList);
                
            }
        }

        private string _currentCriteria = string.Empty;
        public string CurrentCriteria  { 
            get { return _currentCriteria; }
            set { _currentCriteria = value;
                NotifyOfPropertyChange(() => CurrentCriteria);
                NotifyOfPropertyChange(() => HasCriteria);
                ApplyFilter();
            }
        }

        public bool HasCriteria
        {
            get { return _currentCriteria.Length > 0; }
        }

        public void ClearCriteria()
        {
            CurrentCriteria = string.Empty;
        }
        private void ApplyFilter()
        {
            if (Tables == null) return;
            foreach (var node in Tables)
                node.ApplyCriteria(CurrentCriteria, new Stack<FilterableTreeViewItem>());
        }

    }
}
