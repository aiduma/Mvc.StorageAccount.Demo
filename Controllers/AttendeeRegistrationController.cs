using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mvc.StorageAccount.Demo.Data;
using Mvc.StorageAccount.Demo.Models;
using Mvc.StorageAccount.Demo.Services;
using NuGet.Packaging.Signing;
using System.ComponentModel.DataAnnotations;

namespace Mvc.StorageAccount.Demo.Controllers
{
    public class AttendeeRegistrationController : Controller
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IQueueService _queueService;

        public AttendeeRegistrationController(ITableStorageService tableStorageService,
            IBlobStorageService blobStorageService,
            IQueueService queueService)
        {
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
            _queueService = queueService;
        }

        // GET: AttendeeRegistratioController
        public ActionResult Index()
        {
            var data = _tableStorageService.GetAttendees();
            foreach(var item in data)
            {;
                item.ImageName = _blobStorageService.GetBlobUrl(item.ImageName);
            }
            return View(data);
        }

        // GET: AttendeeRegistratioController/Details/5
        public async Task<ActionResult> Details(string id, string industry)
        {
            var data = await _tableStorageService.GetAttendee(industry, id);
            data.ImageName = _blobStorageService.GetBlobUrl(data.ImageName);
            return View(data);
        }

        // GET: AttendeeRegistratioController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AttendeeRegistratioController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AttendeeEntity attendeeEntity,
            IFormFile formFile)
        {
            try
            {
                var id = Guid.NewGuid().ToString();
                attendeeEntity.PartitionKey = attendeeEntity.Industry;
                attendeeEntity.RowKey = id;

                if (formFile?.Length > 0)
                {
                    attendeeEntity.ImageName = await _blobStorageService.UploadBlob(formFile, id);
                }
                else
                {
                    attendeeEntity.ImageName = "default.jpg";
                }

                await _tableStorageService.UpsertAttendee(attendeeEntity);

                var email = new EmailMessage
                {
                    EmailAddress = attendeeEntity.EmailAddress,
                    TimeStamp = DateTime.UtcNow,
                    Message = $"Hello {attendeeEntity.FirstName} {attendeeEntity.LastName}, " +
                        $"\n\r Thank you for registering fot this event." +
                        $"\n\r Your record has been saved for future reference."
                };

                await _queueService.SendMessage(email);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AttendeeRegistratioController/Edit/5
        public async Task<ActionResult> Edit(string id, string industry)
        {
            var data = await _tableStorageService.GetAttendee(industry, id);
            return View(data);
        }

        // POST: AttendeeRegistratioController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, AttendeeEntity attendeeEntity, IFormFile formFile)
        {
            try
            {
                attendeeEntity.PartitionKey = attendeeEntity.Industry;
                attendeeEntity.ImageName = attendeeEntity.RowKey;
                await _tableStorageService.UpsertAttendee(attendeeEntity);

                if (formFile?.Length > 0)
                {
                    await _blobStorageService.UploadBlob(formFile, attendeeEntity.RowKey);
                }

                var email = new EmailMessage
                {
                    EmailAddress = attendeeEntity.EmailAddress,
                    TimeStamp = DateTime.UtcNow,
                    Message = $"Hello {attendeeEntity.FirstName} {attendeeEntity.LastName}, " +
                        $"\n\r Your record was modified successfully."
                };

                await _queueService.SendMessage(email);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AttendeeRegistratioController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View(data);
        //}


        // POST: AttendeeRegistratioController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id, string industry)
        {
            try
            {
                var data = await _tableStorageService.GetAttendee(industry, id);

                await _tableStorageService.DeleteAttendee(industry, id);
                await _blobStorageService.RemoveBlob(data.ImageName);

                var email = new EmailMessage
                {
                    EmailAddress = data.EmailAddress,
                    TimeStamp = DateTime.UtcNow,
                    Message = $"Hello {data.FirstName} {data.LastName}, " +
                        $"\n\r Your record was removed successfully."
                };

                await _queueService.SendMessage(email);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
