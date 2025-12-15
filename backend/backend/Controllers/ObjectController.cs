using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Contracts;
using System.Linq;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly BmsContext _context;
        
        public ObjectController(BmsContext context)
        {
            _context = context;
        }

        [HttpGet("all")] // /api/Object/all
        public ActionResult<List<ObjectWithDevicesResponse>> GetAll()
        {
            var vendorDict = _context.Vendors.ToDictionary(v => v.Id, v => v.Name);
            var modelDict = _context.VendorModels.ToDictionary(m => m.Id, m => m.Name);

            var objects = _context.Objects
                .Select(o => new ObjectWithDevicesResponse
                {
                    Id = o.Id,
                    Name = o.Name,
                    Place = o.Place,
                    Comment = o.Comment,
                    Devices = o.Devices.Select(d => new DeviceShortResponse
                    {
                        Id = d.Id,
                        Name = d.Name,
                        SerialNo = d.SerialNo,
                        Vendor = d.Vendor != null && vendorDict.ContainsKey(d.Vendor.Value) ? vendorDict[d.Vendor.Value] : null,
                        Model = d.Model != null && modelDict.ContainsKey(d.Model.Value) ? modelDict[d.Model.Value] : null,
                        Channel = d.ChannelId.ToString(),
                        InstallDate = d.InstallationDate, // дата установки
                        Comment = d.Comment,
                        TrustedBefore = d.TrustedBefore
                    }).ToList()
                })
                .ToList();
            
            return Ok(objects);
        }
    }
} 