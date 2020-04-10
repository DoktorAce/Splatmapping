using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    List<Vector3> _vertices = new List<Vector3>();
    List<int> _triangles = new List<int>();
    List<Vector2> _uvs = new List<Vector2>();
    List<Color> _colors = new List<Color>();
    List<Vector4> _textures = new List<Vector4>();

    public Mesh Generate(byte[,] bytes)
    {
        _vertices.Clear();
        _triangles.Clear();
        _uvs.Clear();
        _colors.Clear();
        _textures.Clear();

        Random.InitState(999);

        for(var x = 0; x < bytes.GetLength(0); x++)
        {
             for(var y = 0; y < bytes.GetLength(1); y++)
            {
                AddQuad(x, y, bytes);
            }
        }

        // Create mesh
        var mesh = new Mesh();
        mesh.SetVertices(_vertices);
        mesh.SetTriangles(_triangles, 0);
        mesh.SetUVs(0, _uvs);
        mesh.SetUVs(1, _textures);
        mesh.SetColors(_colors);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    struct SplatVector
    {
        public Vector3 pos;
        public byte tex;
    }

    SplatVector GetSplatVector(int x, int y, byte[,] bytes)
    {
        var pos = new Vector3(x, y);
        x = Mathf.Clamp(x, 0, bytes.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, bytes.GetLength(1) - 1);
        return new SplatVector
        {
            pos = pos,
            tex = bytes[x, y],
        };
    }

    void AddQuad(int x, int y, byte[,] bytes)
    {
        var center = GetSplatVector(x, y, bytes);
        var west = GetSplatVector(x-1, y, bytes);
        var northWest = GetSplatVector(x-1, y-1, bytes);
        var north = GetSplatVector(x, y-1, bytes);
        var northEast = GetSplatVector(x+1, y-1, bytes);
        var east = GetSplatVector(x+1, y, bytes);
        var southEast = GetSplatVector(x+1, y+1, bytes);
        var south = GetSplatVector(x, y+1, bytes);
        var southWest = GetSplatVector(x-1, y+1, bytes);

        AddQuarterQuad(center, north, northWest, west, Random.value > 0.5f);
        AddQuarterQuad(center, west, southWest, south, Random.value > 0.5f);
        AddQuarterQuad(center, south, southEast, east, Random.value > 0.5f);
        AddQuarterQuad(center, east, northEast, north, Random.value > 0.5f);
    }

    void AddQuarterQuad(SplatVector v0, SplatVector v1, SplatVector v2, SplatVector v3, bool isFlipped)
    {
        var vCount = _vertices.Count;

        var pos0 = v0.pos;
        var pos1 = Vector3.Lerp(v0.pos, v1.pos, 0.5f);
        var pos2 = Vector3.Lerp(v0.pos, v2.pos, 0.5f);
        var pos3 = Vector3.Lerp(v0.pos, v3.pos, 0.5f);

        _vertices.Add(pos0);
        _vertices.Add(pos1);
        _vertices.Add(pos2);
        _vertices.Add(pos3);

        if(isFlipped)
        {
            _triangles.Add(vCount);
            _triangles.Add(vCount + 1);
            _triangles.Add(vCount + 2);
            _triangles.Add(vCount);
            _triangles.Add(vCount + 2);
            _triangles.Add(vCount + 3);
        }
        else
        {
            _triangles.Add(vCount);
            _triangles.Add(vCount + 1);
            _triangles.Add(vCount + 3);
            _triangles.Add(vCount + 3);
            _triangles.Add(vCount + 1);
            _triangles.Add(vCount + 2);
        }

        _uvs.Add(pos0);
        _uvs.Add(pos1);
        _uvs.Add(pos2);
        _uvs.Add(pos3);

        var blend0 = (1f + (v1.tex == v0.tex ? 1 : 0) + (v2.tex == v0.tex ? 1 : 0) + (v3.tex == v0.tex ? 1 : 0)) / 4f;
        var blend1 = (1f + (v0.tex == v1.tex ? 1 : 0) + (v2.tex == v1.tex ? 1 : 0) + (v3.tex == v1.tex ? 1 : 0)) / 4f;
        var blend2 = (1f + (v0.tex == v2.tex ? 1 : 0) + (v1.tex == v2.tex ? 1 : 0) + (v3.tex == v2.tex ? 1 : 0)) / 4f;
        var blend3 = (1f + (v0.tex == v3.tex ? 1 : 0) + (v1.tex == v3.tex ? 1 : 0) + (v2.tex == v3.tex ? 1 : 0)) / 4f;

        _colors.Add(new Color(0.5f, 0.25f, 0f, 0.25f));
        _colors.Add(new Color(0.5f, 0.5f, 0f, 0f));
        _colors.Add(new Color(blend0, blend1, blend2, blend3));
        _colors.Add(new Color(0.5f, 0f, 0f, 0.5f));

        var textures = new Vector4(v0.tex, v1.tex, v2.tex, v3.tex);
        _textures.Add(textures);
        _textures.Add(textures);
        _textures.Add(textures);
        _textures.Add(textures);
    }


    /*
        /// <summary>
        /// An array of directions to all neighbours
        /// </summary>
        public static readonly int2[] Neighbours = new int2[]
        {
            new int2(-1, 0), // West
            new int2(-1, 1), // North West
            new int2(0, 1), // North
            new int2(1, 1), // North East
            new int2(1, 0), // East
            new int2(1, -1), // South East
            new int2(0, -1), // South
            new int2(-1, -1), // South West
        };

        void DrawQuad(TerrainPointContainer tpc)
        {
            var random = new Random(tpc.center.randomSeed);
            var flipRandom = random.NextBool4();
            DrawQuarter(tpc.center, tpc.neighbor7, tpc.neighbor8, tpc.neighbor1, flipRandom.x);
            DrawQuarter(tpc.center, tpc.neighbor1, tpc.neighbor2, tpc.neighbor3, flipRandom.y);
            DrawQuarter(tpc.center, tpc.neighbor3, tpc.neighbor4, tpc.neighbor5, flipRandom.z);
            DrawQuarter(tpc.center, tpc.neighbor5, tpc.neighbor6, tpc.neighbor7, flipRandom.w);
        }
        
        void DrawQuarter(TerrainPoint t0, TerrainPoint t1, TerrainPoint t2, TerrainPoint t3, bool flipped)
        {
            var v0 = new SplatVertice() { position = t0.position };
            var v1 = new SplatVertice() { position = math.lerp(t0.position, t1.position, 0.5f) };
            var v2 = new SplatVertice() { position = math.lerp(t0.position, t2.position, 0.5f) };
            var v3 = new SplatVertice() { position = math.lerp(t0.position, t3.position, 0.5f) };

            v1.position.y = v0.position.y;
            v2.position.y = v0.position.y;
            v3.position.y = v0.position.y;

            var tex0 = t0.topTexture;
            var tex1 = t1.topTexture;
            var tex2 = t2.topTexture;
            var tex3 = t3.topTexture;
            
            v0 = v0.SetTextures(tex0, tex1, tex2, tex3);
            v1 = v1.SetTextures(tex0, tex1, tex2, tex3);
            v2 = v2.SetTextures(tex0, tex1, tex2, tex3);
            v3 = v3.SetTextures(tex0, tex1, tex2, tex3);

            var tex0Blend = (1f + (tex1 == tex0 ? 1 : 0) + (tex2 == tex0 ? 1 : 0) + (tex3 == tex0 ? 1 : 0)) / 4f;
            var tex1Blend = (1f + (tex0 == tex1 ? 1 : 0) + (tex2 == tex1 ? 1 : 0) + (tex3 == tex1 ? 1 : 0)) / 4f;
            var tex2Blend = (1f + (tex0 == tex2 ? 1 : 0) + (tex1 == tex2 ? 1 : 0) + (tex3 == tex2 ? 1 : 0)) / 4f;
            var tex3Blend = (1f + (tex0 == tex3 ? 1 : 0) + (tex1 == tex3 ? 1 : 0) + (tex2 == tex3 ? 1 : 0)) / 4f;
            
            v0 = v0.SetBlends(0.5f, 0.25f, 0, 0.25f);
            v1 = v1.SetBlends(0.5f, 0.5f, 0, 0);
            v2 = v2.SetBlends(tex0Blend, tex1Blend, tex2Blend, tex3Blend);
            v3 = v3.SetBlends(0.5f, 0, 0, 0.5f);

            v0 = v0.SetProjection(up, up, up, up, zero, zero, zero, zero);
            v0 = v0.ProjectNormals(settings.uvScale);
            v1 = v1.SetProjection(up, up, up, up, zero, zero, zero, zero);
            v1 = v1.ProjectNormals(settings.uvScale);
            v2 = v2.SetProjection(up, up, up, up, zero, zero, zero, zero);
            v2 = v2.ProjectNormals(settings.uvScale);
            v3 = v3.SetProjection(up, up, up, up, zero, zero, zero, zero);
            v3 = v3.ProjectNormals(settings.uvScale);

            v0.channel1.extras = 1f; // Gah.. 1 should be 0.5 but then it looks different than before.. must have fucked up adventure when working on battle
            v1.channel1.extras = 1f;
            v2.channel1.extras = 1f;
            v3.channel1.extras = 1f;

            mesh.AddQuad(v0, v1, v2, v3, flipped);
        }
    */
}