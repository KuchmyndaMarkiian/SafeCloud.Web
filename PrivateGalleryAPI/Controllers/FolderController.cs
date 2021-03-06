﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SafeCloud.Common.BindingModels;
using SafeCloud.DAL.Entities;
using SafeCloud.DAL.Infrastructure;
using FileStructure = SafeCloud.Common.BindingModels.FileStructure;

namespace SafeCloud.API.Controllers
{
    [System.Web.Http.Authorize]
    [System.Web.Http.RoutePrefix("api/Folder")]
    public class FolderController : ApiControllerBase
    {
        // GET api/Folder?name=()
        [System.Web.Http.HttpGet]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [ValidateAntiForgeryToken]
        [ResponseType(typeof(FolderBindindModel))]
        public async Task<IHttpActionResult> Get(string id)
        {
            var user = await UnitOfWork.UserRepository.GetAsync(x => x.UserName == User.Identity.Name);
            if (user == null)
            {
                return InternalServerError();
            }
            if (user.Folders.Any(x => x.Id == id))
            {
                var found = user.Folders.FirstOrDefault(x => x.Id == id);
                return Ok(new FolderBindindModel
                {
                    Name = found.Header,
                    Id = id,
                    DateTime = found.CreatedDate.Value,
                    AttributeHasPublicAccess=found.AttributeHasPublicAccess
                });
            }
            return NotFound();
        }

        // GET api/Folder/List
        [System.Web.Http.HttpGet]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [ValidateAntiForgeryToken]
        [System.Web.Http.Route("List")]
        [ResponseType(typeof(ICollection<FolderBindindModel>))]
        public async Task<IHttpActionResult> List(FolderBindindModel model)
        {
            var user = await UnitOfWork.UserRepository.GetAsync(x => x.UserName == User.Identity.Name);
            if (user == null)
            {
                return InternalServerError();
            }
            if (user.Folders.Any())
            {
                Folder main = user.Folders.FirstOrDefault(x => x.Id == user.Id);
                var structures = new List<FolderBindindModel>(user.Folders.Where(f => f.Id != user.Id)
                    .Select(item => new FileStructure
                    {
                        Id = item.Id,
                        ParentId = item.ParentFolder?.Id,
                        Name = item.Header,
                        DateTime = item.CreatedDate.Value,
                        AttributeHasPublicAccess = item.AttributeHasPublicAccess,
                        Files = item.Files.Select(file => new FileBindingModel
                        {
                            Id = file.Id,
                            ParentId = file.ParentFolder?.Id,
                            Name = file.Header,
                            DateTime = file.CreatedDate.Value,
                            Description = file.Description,
                            Size = file.Size,
                            Geolocation = file.Geolocation,
                            AttributeHasPublicAccess = file.AttributeHasPublicAccess,
                        })
                    }).ToList());
                foreach (var file in UnitOfWork.FileRepository.GetAll())
                {
                    if (file.ParentFolder.Id == main.Id)
                    {
                        structures.Add(
                            new FileBindingModel
                            {
                                Id = file.Id,
                                ParentId = file.ParentFolder?.Id,
                                Name = file.Header,
                                DateTime = file.CreatedDate.Value,
                                Description= file.Description,
                                Geolocation = file.Geolocation,
                                Size = file.Size,
                                AttributeHasPublicAccess = file.AttributeHasPublicAccess,
                    });
                    }
                }
                return Ok(structures);
            }
            return NotFound();
        }
        // GET api/Folder/Structure
        [System.Web.Http.HttpGet]
        [RequireHttps]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [ValidateAntiForgeryToken]
        [System.Web.Http.Route("Structure")]
        [ResponseType(typeof(ICollection<FolderBindindModel>))]
        public async Task<IHttpActionResult> Structure([FromUri][Bind(Include = nameof(FolderBindindModel.Id))]FolderBindindModel model)
        {
            var user = await UnitOfWork.UserRepository.GetAsync(x => x.UserName == User.Identity.Name);
            if (user == null)
            {
                return InternalServerError();
            }
            String folderId = model.Id??user.Id;
            if (user.Folders.Any(x=>x.Id==folderId))
            {
                Folder foundFolder = UnitOfWork.FolderRepository.Get(x => x.Id == folderId && x.OwnerUser==user);
                var structure= new LinkedList<FolderBindindModel>();
                foreach (var item in UnitOfWork.FolderRepository.GetAll(f=>f.ParentFolder==foundFolder))
                {
                    structure.AddLast(new FolderBindindModel
                    {
                        Id = item.Id,
                        ParentId = item.ParentFolder?.Id,
                        Name = item.Header,
                        DateTime = item.CreatedDate.Value,
                        AttributeHasPublicAccess = item.AttributeHasPublicAccess
                    });
                }
                foundFolder.Files.ForEach(file =>
                {
                    structure.AddLast(new FileBindingModel
                    {
                        Id = file.Id,
                        ParentId = file.ParentFolder?.Id,
                        Name = file.Header,
                        DateTime = file.CreatedDate.Value,
                        Description = file.Description,
                        Size = file.Size,
                        Geolocation = file.Geolocation,
                        AttributeHasPublicAccess = file.AttributeHasPublicAccess,
                    });
                });
                //var structures = new List<FolderBindindModel>(user.Folders.Where(f => f.Id != user.Id)
                //    .Select(item => new FileStructure
                //    {
                //        Id = item.Id,
                //        ParentId = item.ParentFolder?.Id,
                //        Name = item.Header,
                //        DateTime = item.CreatedDate.Value,
                //        AttributeHasPublicAccess = item.AttributeHasPublicAccess,
                //        Files = item.Files.Select(file => new FileBindingModel
                //        {
                //            Id = file.Id,
                //            ParentId = file.ParentFolder?.Id,
                //            Name = file.Header,
                //            DateTime = file.CreatedDate.Value,
                //            Description = file.Description,
                //            Size = file.Size,
                //            Geolocation = file.Geolocation,
                //            AttributeHasPublicAccess = file.AttributeHasPublicAccess,
                //        })
                //    }).ToList());
                //foreach (var file in UnitOfWork.FileRepository.GetAll())
                //{
                //    if (file.ParentFolder.Id == foundFolder.Id)
                //    {
                //        structures.Add(
                //            new FileBindingModel
                //            {
                //                Id = file.Id,
                //                ParentId = file.ParentFolder?.Id,
                //                Name = file.Header,
                //                DateTime = file.CreatedDate.Value,
                //                Description = file.Description,
                //                Geolocation = file.Geolocation,
                //                Size = file.Size,
                //                AttributeHasPublicAccess = file.AttributeHasPublicAccess,
                //            });
                //    }
                //}
                return Ok(structure);
            }
            return NotFound();
        }

        // PUT api/Folder/
        [System.Web.Http.HttpPut]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [ValidateAntiForgeryToken]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> Put(
            FolderBindindModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = await UnitOfWork.UserRepository.GetAsync(x => x.UserName == User.Identity.Name);
                if (user != null)
                {
                    if (!user.Folders.Any())
                    {
                        user.Folders.Add(new Folder { Id = user.Id, Header = $"{user.FirstName} {user.LastName}" });
                    }
                    var newItem = DalEntityCreator.CreateGalleryEntity(model);
                    newItem.OwnerUser = user;
                    if (newItem.ParentFolder == null)
                    {
                        newItem.ParentFolder = user.Folders.FirstOrDefault(x => x.Id == user.Id);
                    }
                    UnitOfWork.FolderRepository.Create(newItem);
                    UnitOfWork.SaveAsync();
                    return Ok(newItem.Id);
                }
                return InternalServerError();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // PATCH api/Folder/
        [System.Web.Http.HttpPatch]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [ValidateAntiForgeryToken]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> Patch(
            [Bind(Exclude = nameof(FolderBindindModel.DateTime))] FolderBindindModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = await UnitOfWork.UserRepository.GetAsync(x => x.UserName == User.Identity.Name);
                if (user == null)
                {
                    return InternalServerError();
                }
                if (user.Folders.Any(x => x.Id == model.Id))
                {
                    var found = await UnitOfWork.FolderRepository.GetAsync(gallery => gallery.Id == model.Id);
                    found.Header = model.Name;
                    found.Description = model.Description;
                    UnitOfWork.FolderRepository.Update(found);
                    UnitOfWork.SaveAsync();
                    return Ok("Renamed");
                }
                return NotFound();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // DELETE api/Folder
        [System.Web.Http.HttpDelete]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [ValidateAntiForgeryToken]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> Delete(
             [Bind(Include = nameof(FolderBindindModel.Id))] FolderBindindModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string username = User.Identity.Name;
                var user = UnitOfWork.UserRepository.Get(x => x.UserName == username);
                if (user == null)
                {
                    return BadRequest("Not authorized");
                }
                if (user.Folders.Any(x => x.Id == model.Id))
                {
                    UnitOfWork.FolderRepository.Delete(gallery => gallery.Id == model.Id);
                    UnitOfWork.SaveAsync();
                    return Ok("Deleted");
                }
                return BadRequest("It isn`t found");
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}