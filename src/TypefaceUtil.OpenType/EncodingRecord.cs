﻿using System;

namespace TypefaceUtil.OpenType;

internal struct EncodingRecord
{
    public UInt16 platformID;
    public UInt16 encodingID;
    public UInt32 offset;
}