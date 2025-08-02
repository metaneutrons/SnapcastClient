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

/// <summary>
/// Query parameters for a stream configuration
/// </summary>
public struct StreamQuery
{
    /// <summary>
    /// Gets the chunk size in milliseconds for the stream
    /// </summary>
    [JsonProperty("chunk_ms")]
    public string ChunkMs;

    /// <summary>
    /// Gets the audio codec used by the stream
    /// </summary>
    [JsonProperty("codec")]
    public string Codec;

    /// <summary>
    /// Gets the name of the stream
    /// </summary>
    [JsonProperty("name")]
    public string Name;

    /// <summary>
    /// Gets the sample format specification for the stream
    /// </summary>
    [JsonProperty("sampleformat")]
    public string SampleFormat;
}

/// <summary>
/// URI information for a stream source
/// </summary>
public struct Uri
{
    /// <summary>
    /// Gets the path component of the URI
    /// </summary>
    [JsonProperty("path")]
    public string Path;

    /// <summary>
    /// Gets the scheme component of the URI (e.g., pipe, tcp)
    /// </summary>
    [JsonProperty("scheme")]
    public string Scheme;

    /// <summary>
    /// Gets the query parameters for the stream
    /// </summary>
    [JsonProperty("query")]
    public StreamQuery Query;

    /// <summary>
    /// Gets the fragment component of the URI
    /// </summary>
    [JsonProperty("fragment")]
    public string Fragment;

    /// <summary>
    /// Gets the host component of the URI
    /// </summary>
    [JsonProperty("host")]
    public string Host;

    /// <summary>
    /// Gets the raw URI string
    /// </summary>
    [JsonProperty("raw")]
    public string Raw;
}

/// <summary>
/// Binary data for album artwork
/// </summary>
public struct ArtData
{
    /// <summary>
    /// Gets the base64-encoded artwork data
    /// </summary>
    [JsonProperty("data")]
    public string Data;

    /// <summary>
    /// Gets the file extension of the artwork (e.g., jpg, png)
    /// </summary>
    [JsonProperty("extension")]
    public string extension;
}

/// <summary>
/// Metadata information for a stream
/// </summary>
public struct StreamMetadata
{
    /// <summary>
    /// Gets the album name
    /// </summary>
    [JsonProperty("album")]
    public string Album;

    /// <summary>
    /// Gets the list of artists
    /// </summary>
    [JsonProperty("artist")]
    public List<string> Artist;

    /// <summary>
    /// Gets the track title
    /// </summary>
    [JsonProperty("title")]
    public string Title;

    /// <summary>
    /// Gets the URL to the album artwork
    /// </summary>
    [JsonProperty("artUrl")]
    public string ArtUrl;

    /// <summary>
    /// Gets the binary album artwork data
    /// </summary>
    [JsonProperty("artData")]
    public ArtData ArtData;
}

/// <summary>
/// Playback control properties for a stream
/// </summary>
public struct StreamProperties
{
    /// <summary>
    /// Gets a value indicating whether the stream can be controlled
    /// </summary>
    [JsonProperty("canControl")]
    public bool CanControl;

    /// <summary>
    /// Gets a value indicating whether the stream supports going to the next track
    /// </summary>
    [JsonProperty("canGoNext")]
    public bool CanGoNext;

    /// <summary>
    /// Gets a value indicating whether the stream supports going to the previous track
    /// </summary>
    [JsonProperty("canGoPrevious")]
    public bool CanGoPrevious;

    /// <summary>
    /// Gets a value indicating whether the stream can be paused
    /// </summary>
    [JsonProperty("canPause")]
    public bool CanPause;

    /// <summary>
    /// Gets a value indicating whether the stream can be played
    /// </summary>
    [JsonProperty("canPlay")]
    public bool CanPlay;

    /// <summary>
    /// Gets a value indicating whether the stream supports seeking
    /// </summary>
    [JsonProperty("canSeek")]
    public bool CanSeek;

    /// <summary>
    /// Gets the metadata for the currently playing track
    /// </summary>
    [JsonProperty("metadata")]
    public StreamMetadata? Metadata;
}

/// <summary>
/// Represents a complete Snapcast stream with all its properties
/// </summary>
public struct Stream
{
    /// <summary>
    /// Gets the unique identifier of the stream
    /// </summary>
    [JsonProperty("id")]
    public string Id;

    /// <summary>
    /// Gets the current status of the stream (e.g., playing, idle)
    /// </summary>
    [JsonProperty("status")]
    public string Status;

    /// <summary>
    /// Gets the playback control properties of the stream
    /// </summary>
    [JsonProperty("properties")]
    public StreamProperties? Properties;

    /// <summary>
    /// Gets the URI information for the stream source
    /// </summary>
    [JsonProperty("uri")]
    public Uri Uri;
}
