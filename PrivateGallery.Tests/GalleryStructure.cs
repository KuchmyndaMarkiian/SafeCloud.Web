﻿using System;
using System.Collections.Generic;

namespace SafeCloud.Tests
{
    public class GalleryStructure: IStructure
    {
        public IEnumerable<PictureStructure> Pictures { get; set; }
        public string Name { get; set; }
        public DateTime DateTime { get; set; }
    }
}