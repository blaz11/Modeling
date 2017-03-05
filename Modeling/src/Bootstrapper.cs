using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Modeling.Graphics;
using Modeling.Screens;

namespace Modeling
{
    public class Bootstrapper : BootstrapperBase
    {
        private readonly SimpleContainer _container = new SimpleContainer();

        public Bootstrapper()
        {
            RegisterComponents();
            Initialize();
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            return _container.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
           var settings = new Dictionary<string, object>
           {
               //{ "SizeToContent", SizeToContent.WidthAndHeight },
               //{ "Height" , 732 },
               //{ "Width"  , 1056 },
               //{ "SizeChanged", () => a }
           };
            //DisplayRootViewFor<ModelingMainViewModel>(settings);

            //var context = _container.GetInstance(typeof(ModelingMainViewModel), string.Empty);
            //var view = new ModelingMainView {DataContext = context };
            //view.Show();

            var bitmapMainView = new BitmapMainView();
            bitmapMainView.Show();
        }

        private void a(object a, object b)
        {
            
        }

        private void RegisterComponents()
        {
            _container.RegisterInstance(typeof(IWindowManager), string.Empty, new WindowManager());
            _container.RegisterInstance(typeof(IEventAggregator), string.Empty, new EventAggregator());

            var camera = new Camera();
            BuildUp(camera);
            _container.RegisterInstance(typeof(Camera), string.Empty, camera);

            //var scene = new Scene();
            //BuildUp(scene);
            //_container.RegisterInstance(typeof(IScene), string.Empty, scene);

            var modelingMainViewModel = new ModelingMainViewModel();
            BuildUp(modelingMainViewModel);
            modelingMainViewModel.Camera = camera;
            _container.RegisterInstance(typeof(ModelingMainViewModel), string.Empty, modelingMainViewModel);
        }
    }
}