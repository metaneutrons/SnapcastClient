/***
    This file is part of snapcast-net
    Copyright (C) 2024  Craig Sturdy

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
***/

using Newtonsoft.Json;

namespace SnapcastClient.Models;

public struct StreamQuery
{
    [JsonProperty("chunk_ms")]
    public string ChunkMs;

    [JsonProperty("codec")]
    public string Codec;

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("sampleformat")]
    public string SampleFormat;
}

public struct Uri
{
    [JsonProperty("path")]
    public string Path;

    [JsonProperty("scheme")]
    public string Scheme;

    [JsonProperty("query")]
    public StreamQuery Query;

    [JsonProperty("fragment")]
    public string Fragment;

    [JsonProperty("host")]
    public string Host;

    [JsonProperty("raw")]
    public string Raw;
}

public struct ArtData
{
    [JsonProperty("data")]
    public string Data;

    [JsonProperty("extension")]
    public string extension;
}

public struct StreamMetadata
{
    [JsonProperty("album")]
    public string Album;

    [JsonProperty("artist")]
    public List<string> Artist;

    [JsonProperty("title")]
    public string Title;

    [JsonProperty("artUrl")]
    public string ArtUrl;

    [JsonProperty("artData")]
    public ArtData ArtData;
}

public struct StreamProperties
{
    [JsonProperty("canControl")]
    public bool CanControl;

    [JsonProperty("canGoNext")]
    public bool CanGoNext;

    [JsonProperty("canGoPrevious")]
    public bool CanGoPrevious;

    [JsonProperty("canPause")]
    public bool CanPause;

    [JsonProperty("canPlay")]
    public bool CanPlay;

    [JsonProperty("canSeek")]
    public bool CanSeek;

    [JsonProperty("metadata")]
    public StreamMetadata? Metadata;
}

public struct Stream
{
    [JsonProperty("id")]
    public string Id;

    [JsonProperty("status")]
    public string Status;

    [JsonProperty("properties")]
    public StreamProperties? Properties;

    [JsonProperty("uri")]
    public Uri Uri;
}
