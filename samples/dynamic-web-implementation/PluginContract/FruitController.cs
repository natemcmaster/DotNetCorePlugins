using Microsoft.AspNetCore.Mvc;
using PluginLib.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PluginLib
{
    [Route("api/fruit")]
    [ApiController]
    public class FruitController: ControllerBase
    {
        private readonly IFruitService _fruitService;
        public FruitController(IFruitService service)
        {
           _fruitService = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Fruit>> Get()
        {
            return _fruitService.GetFruits();
        }
    }
}
