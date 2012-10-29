﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AvalonDock.Layout;
using BiM.Core.Collections;
using BiM.Core.Reflection;
using BiM.Host.UI.ViewModels;
using BiM.Host.UI.Views;

namespace BiM.Host.UI
{
    public class UIManager : Singleton<UIManager>
    {
        private ObservableCollectionMT<object> m_documents = new ObservableCollectionMT<object>();
        private ReadOnlyObservableCollectionMT<object> m_readOnlyDocuments; 
        private Dictionary<object, Assembly> m_documentsAssembly = new Dictionary<object, Assembly>();

        public UIManager()
        {
            m_readOnlyDocuments = new ReadOnlyObservableCollectionMT<object>(m_documents);
            m_documents.CollectionChanged += OnDocumentsChanged;
            DocumentTemplateSelector = new DocumentTemplateSelector();
            DocumentStyleSelector = new DocumentStyleSelector();

            MainWindow.Initialized += OnInitialized;

        }

        private void OnInitialized(object sender, EventArgs e)
        {
            InitializeSelectors();

            Task.Factory.StartNew(() =>
                              {
                                  Host.Initialize();
                                  Host.Start();
                              });
        }

        private void InitializeSelectors()
        {
            DocumentTemplateSelector.AddTemplate(typeof(BotViewModel), (DataTemplate)MainWindow.Resources["BotViewTemplate"]);
            DocumentStyleSelector.AddStyle(typeof(BotViewModel), (Style)MainWindow.Resources["BotViewStyle"]);
        }

        private void Dispatch(Action action)
        {
            MainWindow.Dispatcher.Invoke(action);
        }

        private void OnDocumentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    m_documentsAssembly.Remove(item);
                }
            }

            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                m_documentsAssembly.Clear();
            }
        }

        public ReadOnlyObservableCollection<object> Documents
        {
            get
            {
                return m_readOnlyDocuments;
            }
        }

        public DocumentTemplateSelector DocumentTemplateSelector
        {
            get;
            private set;
        }

        public DocumentStyleSelector DocumentStyleSelector
        {
            get;
            private set;
        }

        public void AddDocument(object document)
        {
            lock (m_documents)
            {
                m_documents.Add(document);
            }
            if (!m_documentsAssembly.ContainsKey(document))
                m_documentsAssembly.Add(document, Assembly.GetCallingAssembly());
        }

        public void AddDocument(LayoutDocument document)
        {
            lock (m_documents)
            {
                m_documents.Add(document);
            }
            if (!m_documentsAssembly.ContainsKey(document))
                m_documentsAssembly.Add(document, Assembly.GetCallingAssembly());
        }

        public LayoutDocument AddDocument(object document, string title)
        {
            var layout = new LayoutDocument()
            {
                Content = document,
                Title = title,
            };

            lock (m_documents)
            {
                m_documents.Add(document);
            }
            if (!m_documentsAssembly.ContainsKey(document))
                m_documentsAssembly.Add(document, Assembly.GetCallingAssembly());

            return layout;
        }

        public LayoutDocument AddDocument(object document, string title, ImageSource icon)
        {
            var layout = new LayoutDocument()
            {
                Content = document,
                Title = title,
                IconSource = icon
            };

            lock (m_documents)
            {
                m_documents.Add(document);
            }
            if (!m_documentsAssembly.ContainsKey(document))
                m_documentsAssembly.Add(document, Assembly.GetCallingAssembly());

            return layout;
        }
        
        public void AddDocument(object document, DataTemplate template)
        {
            if (!DocumentTemplateSelector.HasTemplate(document))
            {
                DocumentTemplateSelector.AddTemplate(document, template);
            }

            lock (m_documents)
            {
                m_documents.Add(document);
            }
            if (!m_documentsAssembly.ContainsKey(document))
                m_documentsAssembly.Add(document, Assembly.GetCallingAssembly());
        }

        public void AddDocument(object document, Style style)
        {
            if (!DocumentStyleSelector.HasStyle(document))
            {
                DocumentStyleSelector.AddStyle(document, style);
            }

            lock (m_documents)
            {
                m_documents.Add(document);
            }
            if (!m_documentsAssembly.ContainsKey(document))
                m_documentsAssembly.Add(document, Assembly.GetCallingAssembly());
        }

        public void AddDocument(object document, Style style, DataTemplate template)
        {
            if (!DocumentStyleSelector.HasStyle(document))
            {
                DocumentStyleSelector.AddStyle(document, style);
            }

            if (!DocumentTemplateSelector.HasTemplate(document))
            {
                DocumentTemplateSelector.AddTemplate(document, template);
            }
            lock (m_documents)
            {
                m_documents.Add(document);
            }
            if (!m_documentsAssembly.ContainsKey(document))
                m_documentsAssembly.Add(document, Assembly.GetCallingAssembly());
        }

        public void RemoveDocumentsFrom(Assembly assembly)
        {
            var documentsToRemove = m_documentsAssembly.Where(x => x.Value == assembly).Select(x => x.Key).ToArray();

            foreach (var document in documentsToRemove)
            {
                RemoveDocument(document);
            }
        }

        public bool RemoveDocument(object document)
        {
            bool removed;
            lock (m_documents)
            {
                removed = m_documents.Remove(document);
            }

            if (removed)
            {
                DocumentStyleSelector.RemoveStyle(document);
                DocumentTemplateSelector.RemoveTemplate(document);
            }

            return removed;
        }

        public MainWindow MainWindow
        {
            get
            {
                return (MainWindow)Application.Current.MainWindow;
            }
        }

        public BotViewModel GetBotViewModel(Behaviors.Bot bot)
        {
            lock (m_documents)
            {
                return m_documents.FirstOrDefault(x => x is BotViewModel && ( x as BotViewModel ).Bot == bot) as BotViewModel;
            }
        }

        public BotViewModel[] GetBotsViewModel()
        {
            lock (m_documents)
            {
                return m_documents.OfType<BotViewModel>().ToArray();
            }
        }
    }
}