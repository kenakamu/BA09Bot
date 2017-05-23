using Autofac;
using BA09Bot.Dialogs;
using BA09Bot.Models;
using BA09Bot.Resources;
using BA09Bot.Services;
using Microsoft.Bot.Builder.Base;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BA09Bot.UnitTests
{
    [TestClass]
    public class SampleDialogTest : DialogTestBase
    {
        [TestMethod]
        public async Task ShouldReturnNewAppointmentCard()
        {
            Initialize();
            // Replace Dialog class to your own class
            IDialog<object> rootDialog = new RootDialog();

            var toBot = DialogTestBase.MakeTestMessage();
            toBot.From.Id = Guid.NewGuid().ToString();
            toBot.Text = Resource.Menu_Create_Appointment;
            Func<IDialog<object>> MakeRoot = () => rootDialog;

            using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
            using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
            {
                // act: sending the message
                IMessageActivity toUser = await GetResponse(container, MakeRoot, toBot);
                // assert: check if the dialog returned the right response
                ThumbnailCard card = (ThumbnailCard)toUser.Attachments.First().Content;
                Assert.IsTrue(card.Text.Equals(Resource.Create_Appointment));
                Assert.IsTrue(card.Title.Equals(Resource.Open_In_Browser));
            }
        }

        [TestMethod]
        public async Task ShouldReturnNoAppointmentToday()
        {
            Initialize();
            // Replace Dialog class to your own class
            IDialog<object> rootDialog = new RootDialog();
            var d365Mock = new Mock<ID365Service>();
            d365Mock.Setup(m => m.GetAppointmentsForToday()).Returns(Task.FromResult(new List<Appointment>()));

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(d365Mock.Object).As<ID365Service>();
            WebApiApplication.Container = builder.Build();

            var toBot = DialogTestBase.MakeTestMessage();
            toBot.From.Id = Guid.NewGuid().ToString();
            toBot.Text = Resource.Menu_Appointment_For_Today;
            Func<IDialog<object>> MakeRoot = () => rootDialog;

            using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
            using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
            {
                // act: sending the message
                IMessageActivity toUser = await GetResponse(container, MakeRoot, toBot);
                // assert: check if the dialog returned the right response
                Assert.IsTrue(toUser.Text.Equals(Resource.No_Appointment_Today));
            }
        }

        [TestMethod]
        public async Task ShouldReturnAppointmentToday()
        {
            Initialize();
            // Replace Dialog class to your own class
            IDialog<object> rootDialog = new RootDialog();
            var d365Mock = new Mock<ID365Service>();
            d365Mock.Setup(m => m.GetAppointmentsForToday()).Returns(
                Task.FromResult(
                    new List<Appointment>() {
                        new Appointment()
                        {
                            Title = "title",
                            Description = "desc",
                            FormattedStartDate = DateTime.Parse("2017/01/01 10:00:00"),
                            FormattedEndDate = DateTime.Parse("2017/01/01 11:00:00")
                        }
                    }));

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(d365Mock.Object).As<ID365Service>();
            WebApiApplication.Container = builder.Build();

            var toBot = DialogTestBase.MakeTestMessage();
            toBot.From.Id = Guid.NewGuid().ToString();
            toBot.Text = Resource.Menu_Appointment_For_Today;
            Func<IDialog<object>> MakeRoot = () => rootDialog;

            using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
            using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
            {
                // act: sending the message
                IMessageActivity toUser = await GetResponse(container, MakeRoot, toBot);
                // assert: check if the dialog returned the right response
                Assert.IsTrue(toUser.Text.Equals("10:00 AM-11:00 AM:title"));
            }
        }

        [TestMethod]
        public async Task ShouldReturnProducts()
        {
            Initialize();
            // Replace Dialog class to your own class
            IDialog<object> rootDialog = new RootDialog();
            var d365Mock = new Mock<ID365Service>();
            d365Mock.Setup(m => m.GetProducts("dummy query")).Returns(
                Task.FromResult(
                    new List<Product>() {
                        new Product()
                        {
                            Name = "Dummy Product",
                            Description = "Dummy Desc"
                        }
                    }));

            var azureBlobMock = new Mock<IAzureBlobService>();
            azureBlobMock.Setup(m => m.Upload("", "")).Returns("");

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(d365Mock.Object).As<ID365Service>();
            builder.RegisterInstance(azureBlobMock.Object).As<IAzureBlobService>();
            WebApiApplication.Container = builder.Build();

            var toBot = DialogTestBase.MakeTestMessage();
            toBot.From.Id = Guid.NewGuid().ToString();
            toBot.Text = Resource.Menu_SearchProduct;
            Func<IDialog<object>> MakeRoot = () => rootDialog;

            using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
            using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
            {
                // act: sending the message
                IMessageActivity toUser = await GetResponse(container, MakeRoot, toBot);
                Assert.IsTrue(toUser.Text.Equals(Resource.Enter_ProductName));

                toBot.Text = "dummy query";

                toUser = await GetResponse(container, MakeRoot, toBot);
                ThumbnailCard card = (ThumbnailCard)toUser.Attachments.First().Content;
                // assert: check if the dialog returned the right response
                Assert.IsTrue(card.Title.Equals("Dummy Product"));
                Assert.IsTrue(card.Buttons.First().Title.Equals(Resource.Detail));
            }
        }

        [TestMethod]
        public async Task ShouldReturnTooManyProducts()
        {
            Initialize();
            // Replace Dialog class to your own class
            IDialog<object> rootDialog = new RootDialog();
            var d365Mock = new Mock<ID365Service>();
            d365Mock.Setup(m => m.GetProducts("dummy query")).Returns(
                Task.FromResult(
                    new List<Product>() {
                        new Product()
                        {
                            Name = "Dummy Product",
                            Description = "Dummy Desc"
                        },
                        new Product()
                        {
                            Name = "Dummy Product",
                            Description = "Dummy Desc"
                        },
                        new Product()
                        {
                            Name = "Dummy Product",
                            Description = "Dummy Desc"
                        },
                        new Product()
                        {
                            Name = "Dummy Product",
                            Description = "Dummy Desc"
                        },
                        new Product()
                        {
                            Name = "Dummy Product",
                            Description = "Dummy Desc"
                        },
                        new Product()
                        {
                            Name = "Dummy Product",
                            Description = "Dummy Desc"
                        }
                    }));
                      
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(d365Mock.Object).As<ID365Service>();
            WebApiApplication.Container = builder.Build();

            var toBot = DialogTestBase.MakeTestMessage();
            toBot.From.Id = Guid.NewGuid().ToString();
            toBot.Text = Resource.Menu_SearchProduct;
            Func<IDialog<object>> MakeRoot = () => rootDialog;

            using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
            using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
            {
                // act: sending the message
                IMessageActivity toUser = await GetResponse(container, MakeRoot, toBot);
                Assert.IsTrue(toUser.Text.Equals(Resource.Enter_ProductName));

                toBot.Text = "dummy query";

                toUser = await GetResponse(container, MakeRoot, toBot);
             
                // assert: check if the dialog returned the right response
                Assert.IsTrue(toUser.Text.Equals(string.Format(Resource.Retry_More_Than_x_Items, 5)));
            }
        }

        [TestMethod]
        public async Task ShouldReturnTasks()
        {
            Initialize();
            // Replace Dialog class to your own class
            IDialog<object> rootDialog = new RootDialog();
            var d365Mock = new Mock<ID365Service>();
            d365Mock.Setup(m => m.GetTasks()).Returns(
                Task.FromResult(
                    new List<CrmTask>() {
                        new CrmTask()
                        {
                             Title = "Dummy Task",
                             Description = "Dummy Desc",
                             DueDateTime = DateTime.Parse("2017/01/01 12:00:00"),
                             Type = "D365"
                        },
                        new CrmTask()
                        {
                             Title = "Dummy Task 2",
                             Description = "Dummy Desc 2",
                             DueDateTime = DateTime.Parse("2017/01/01 15:00:00"),
                             Type = "D365"
                        }
                    }));

            var o365Mock = new Mock<IO365Service>();
            o365Mock.Setup(m => m.GetTasks()).Returns(
                Task.FromResult(
                    new List<PlannerTask>() {
                        new PlannerTask()
                        {
                             Title = "Dummy Task 3",
                             Description = "Dummy Desc",
                             DueDateTime = DateTime.Parse("2017/01/01 13:00:00"),
                             Type = "O365"
                        },
                        new PlannerTask()
                        {
                             Title = "Dummy Task 4",
                             Description = "Dummy Desc 2",
                             DueDateTime = DateTime.Parse("2017/01/01 14:00:00"),
                             Type = "O365"
                        }
                    }));

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(d365Mock.Object).As<ID365Service>();
            builder.RegisterInstance(o365Mock.Object).As<IO365Service>();
            WebApiApplication.Container = builder.Build();

            var toBot = DialogTestBase.MakeTestMessage();
            toBot.From.Id = Guid.NewGuid().ToString();
            toBot.Text = Resource.Menu_Task_Menu;
            Func<IDialog<object>> MakeRoot = () => rootDialog;

            using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
            using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
            {
                // act: sending the message
                IMessageActivity toUser = await GetResponse(container, MakeRoot, toBot);
                // assert: check if menu (list) is back
                Assert.IsTrue(toUser.AttachmentLayout.Equals("list"));

                toBot.Text = Resource.Get_Tasks;

                toUser = await GetResponse(container, MakeRoot, toBot);

                // assert: check if the dialog returned the right response
                Assert.IsTrue(toUser.Text.Equals(@"01/01 12:00 Dummy Task
01/01 13:00 Dummy Task 3
01/01 14:00 Dummy Task 4
01/01 15:00 Dummy Task 2
"));
            }
        }

        [TestMethod]
        public async Task ShouldReturnNextTask()
        {
            Initialize();
            // Replace Dialog class to your own class
            IDialog<object> rootDialog = new RootDialog();
            var d365Mock = new Mock<ID365Service>();
            d365Mock.Setup(m => m.GetTasks()).Returns(
                Task.FromResult(
                    new List<CrmTask>() {
                        new CrmTask()
                        {
                             Title = "Dummy Task",
                             Description = "Dummy Desc",
                             DueDateTime = DateTime.Parse("2017/01/01 12:00:00"),
                             Type = "D365"
                        },
                        new CrmTask()
                        {
                             Title = "Dummy Task 2",
                             Description = "Dummy Desc 2",
                             DueDateTime = DateTime.Parse("2017/01/01 15:00:00"),
                             Type = "D365"
                        }
                    }));

            var o365Mock = new Mock<IO365Service>();
            o365Mock.Setup(m => m.GetTasks()).Returns(
                Task.FromResult(
                    new List<PlannerTask>() {
                        new PlannerTask()
                        {
                             Title = "Dummy Task 3",
                             Description = "Dummy Desc",
                             DueDateTime = DateTime.Parse("2017/01/01 13:00:00"),
                             Type = "O365"
                        },
                        new PlannerTask()
                        {
                             Title = "Dummy Task 4",
                             Description = "Dummy Desc 2",
                             DueDateTime = DateTime.Parse("2017/01/01 14:00:00"),
                             Type = "O365"
                        }
                    }));

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(d365Mock.Object).As<ID365Service>();
            builder.RegisterInstance(o365Mock.Object).As<IO365Service>();
            WebApiApplication.Container = builder.Build();

            var toBot = DialogTestBase.MakeTestMessage();
            toBot.From.Id = Guid.NewGuid().ToString();
            toBot.Text = Resource.Menu_Task_Menu;
            Func<IDialog<object>> MakeRoot = () => rootDialog;

            using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
            using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
            {
                // act: sending the message
                IMessageActivity toUser = await GetResponse(container, MakeRoot, toBot);
                // assert: check if menu (list) is back
                Assert.IsTrue(toUser.AttachmentLayout.Equals("list"));

                toBot.Text = Resource.Get_NextTask;

                toUser = await GetResponse(container, MakeRoot, toBot);

                // assert: check if the dialog returned the right response
                Assert.IsTrue(toUser.Text.Equals("01/01 12:00 Dummy Task-Dummy Desc"));

                toBot.Text = Resource.Get_NextTask;

                toUser = await GetResponse(container, MakeRoot, toBot);

                // assert: check if the dialog returned the right response
                Assert.IsTrue(toUser.Text.Equals("01/01 13:00 Dummy Task 3-Dummy Desc"));
            }
        }

        [TestMethod]
        public async Task ShouldBeAbleToAddDailyReport()
        {
            Initialize();
            // Replace Dialog class to your own class
            IDialog<object> rootDialog = new RootDialog();
            var d365Mock = new Mock<ID365Service>();
            d365Mock.Setup(m => m.CreateDailyReport("dummy")).Returns(
                Task.CompletedTask);
            
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(d365Mock.Object).As<ID365Service>();
            WebApiApplication.Container = builder.Build();

            var toBot = DialogTestBase.MakeTestMessage();
            toBot.From.Id = Guid.NewGuid().ToString();
            toBot.Text = Resource.Menu_Daily_Report;

            Func<IDialog<object>> MakeRoot = () => rootDialog;

            using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
            using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
            {
                // act: sending the message
                IMessageActivity toUser = await GetResponse(container, MakeRoot, toBot);
                // assert: check if menu (list) is back
                Assert.IsTrue(toUser.Text.Equals(Resource.Ask_DailyReport));

                toBot.Text = "dummy report";

                toUser = await GetResponse(container, MakeRoot, toBot);

                // assert: check if the dialog returned the right response
                Assert.IsTrue(toUser.Text.Equals(Resource.Created_DailyReport));
            }
        }

        [TestMethod]
        public async Task ShouldBeAbleToReboot()
        {
            Initialize();
            // Replace Dialog class to your own class
            IDialog<object> rootDialog = new SystemAlertDialog("dummy title", "dummy desc", "dummy id", "dummy id");
            var d365Mock = new Mock<ID365Service>();
            d365Mock.Setup(m => m.CreateIoTCommand("dummy operetion", "dummy id", "dummy id")).Returns(
                Task.CompletedTask);

            var imageMock = new Mock<IImageService>();
            imageMock.Setup(m => m.GetImageUri("dummy")).Returns("");

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(d365Mock.Object).As<ID365Service>();
            builder.RegisterInstance(imageMock.Object).As<IImageService>();
            WebApiApplication.Container = builder.Build();

            var toBot = DialogTestBase.MakeTestMessage();
            toBot.From.Id = Guid.NewGuid().ToString();
            toBot.Text = "Reboot";

            Func<IDialog<object>> MakeRoot = () => rootDialog;

            using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
            using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
            {
                List<IMessageActivity> toUser = await GetResponses(container, MakeRoot, toBot);
                // act: sending the message
                // assert: check if menu (list) is back
                Assert.IsTrue(toUser[0].AttachmentLayout.Equals("carousel"));
               
                Assert.IsTrue(toUser[1].Text.Equals(string.Format(Resource.Done_Operation, "Reboot")));
            }
        }

        [TestMethod]
        public async Task ShouldBeAbleToReset()
        {
            Initialize();
            // Replace Dialog class to your own class
            IDialog<object> rootDialog = new SystemAlertDialog("dummy title", "dummy desc", "dummy id", "dummy id");
            var d365Mock = new Mock<ID365Service>();
            d365Mock.Setup(m => m.CreateIoTCommand("dummy operetion", "dummy id", "dummy id")).Returns(
                Task.CompletedTask);

            var imageMock = new Mock<IImageService>();
            imageMock.Setup(m => m.GetImageUri("dummy")).Returns("");

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(d365Mock.Object).As<ID365Service>();
            builder.RegisterInstance(imageMock.Object).As<IImageService>();
            WebApiApplication.Container = builder.Build();

            var toBot = DialogTestBase.MakeTestMessage();
            toBot.From.Id = Guid.NewGuid().ToString();
            toBot.Text = "Reset";

            Func<IDialog<object>> MakeRoot = () => rootDialog;

            using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
            using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
            {
                List<IMessageActivity> toUser = await GetResponses(container, MakeRoot, toBot);
                // act: sending the message
                // assert: check if menu (list) is back
                Assert.IsTrue(toUser[0].AttachmentLayout.Equals("carousel"));

                Assert.IsTrue(toUser[1].Text.Equals(string.Format(Resource.Done_Operation, "Reset")));
            }
        }

        /// <summary>
        /// Send message to bot and returns first result
        /// </summary>
        private async Task<IMessageActivity> GetResponse(IContainer container, Func<IDialog<object>> makeRoot, IMessageActivity toBot)
        {
            using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
            {
                DialogModule_MakeRoot.Register(scope, makeRoot);

                // act: sending the message
                using (new LocalizedScope(toBot.Locale))
                {
                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, CancellationToken.None);
                }
                //await Conversation.SendAsync(toBot, makeRoot, CancellationToken.None);
                return scope.Resolve<Queue<IMessageActivity>>().Dequeue();
            }
        }

        /// <summary>
        /// Send message to bot and returns all results
        /// </summary>
        private async Task<List<IMessageActivity>> GetResponses(IContainer container, Func<IDialog<object>> makeRoot, IMessageActivity toBot)
        {
            using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
            {
                List<IMessageActivity> activities = new List<IMessageActivity>();
                DialogModule_MakeRoot.Register(scope, makeRoot);

                // act: sending the message
                using (new LocalizedScope(toBot.Locale))
                {
                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, CancellationToken.None);
                }
                //await Conversation.SendAsync(toBot, makeRoot, CancellationToken.None);
                var queue = scope.Resolve<Queue<IMessageActivity>>();

                while (queue.Count != 0)
                {
                    activities.Add(queue.Dequeue());
                }
                return activities;
            }
        }

        private IMessageActivity GetResponse(IContainer container, Func<IDialog<object>> makeRoot)
        {

            using (var scope = DialogModule.BeginLifetimeScope(container, DialogTestBase.MakeTestMessage()))
            {
                DialogModule_MakeRoot.Register(scope, makeRoot);

                return scope.Resolve<Queue<IMessageActivity>>().Dequeue();
            }
        }

        private void Initialize()
        {
            AuthBot.Models.AuthSettings.Mode = ConfigurationManager.AppSettings["ActiveDirectory.Mode"];
            AuthBot.Models.AuthSettings.EndpointUrl = ConfigurationManager.AppSettings["ActiveDirectory.EndpointUrl"];
            AuthBot.Models.AuthSettings.Tenant = ConfigurationManager.AppSettings["ActiveDirectory.Tenant"];
            AuthBot.Models.AuthSettings.RedirectUrl = ConfigurationManager.AppSettings["ActiveDirectory.RedirectUrl"];
            AuthBot.Models.AuthSettings.ClientId = ConfigurationManager.AppSettings["ActiveDirectory.ClientId"];
            AuthBot.Models.AuthSettings.ClientSecret = ConfigurationManager.AppSettings["ActiveDirectory.ClientSecret"];
        }
    }
}
