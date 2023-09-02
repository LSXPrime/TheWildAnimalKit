using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LBSE
{
	[Serializable]
	public struct ColorS
	{
		public float r;
		public float g;
		public float b;
		public float a;

		public ColorS ( Color color )
		{
			this.r = color.r;
			this.g = color.g;
			this.b = color.b;
			this.a = color.a;
		}

		public static implicit operator ColorS ( Color color )
		{
			return new ColorS ( color );
		}

		public static implicit operator Color ( ColorS color )
		{
			return new Color ( color.r, color.g, color.b, color.a );
		}
	}
	
	[Serializable]
	public struct Color32S
	{
		public byte r;
		public byte g;
		public byte b;
		public byte a;

		public Color32S(Color32 color)
		{
			this.r = color.r;
			this.g = color.g;
			this.b = color.b;
			this.a = color.a;
		}

		public static implicit operator Color32S(Color32 color)
		{
			return new Color32S(color);
		}

		public static implicit operator Color32(Color32S color)
		{
			return new Color32(color.r, color.g, color.b, color.a);
		}
	}
	
	[Serializable]
	public struct Vector2S
	{
		public float x;
		public float y;

		public Vector2S(float x)
		{
			this.x = x;
			this.y = 0f;
		}

		public Vector2S(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public Vector2S(Vector2 vector)
		{
			this.x = vector.x;
			this.y = vector.y;
		}

		public Vector2S(Vector3 vector)
		{
			this.x = vector.x;
			this.y = vector.y;
		}

		public Vector2S(Vector4 vector)
		{
			this.x = vector.x;
			this.y = vector.y;
		}

		public Vector2S(Vector2S vector)
		{
			this.x = vector.x;
			this.y = vector.y;
		}

		public Vector2S(Vector3S vector)
		{
			this.x = vector.x;
			this.y = vector.y;
		}

		public Vector2S(Vector4S vector)
		{
			this.x = vector.x;
			this.y = vector.y;
		}

		public static implicit operator Vector2S(Vector2 vector)
		{
			return new Vector2S(vector);
		}

		public static implicit operator Vector2(Vector2S vector)
		{
			return new Vector2(vector.x, vector.y);
		}

		public static implicit operator Vector2S(Vector3 vector)
		{
			return new Vector2S(vector);
		}

		public static implicit operator Vector3(Vector2S vector)
		{
			return new Vector3(vector.x, vector.y);
		}

		public static implicit operator Vector2S(Vector4 vector)
		{
			return new Vector2S(vector);
		}

		public static implicit operator Vector4(Vector2S vector)
		{
			return new Vector4(vector.x, vector.y);
		}

		public static implicit operator Vector2S(Vector3S vector)
		{
			return new Vector2S(vector);
		}

		public static implicit operator Vector3S(Vector2S vector)
		{
			return new Vector3S(vector);
		}

		public static implicit operator Vector2S(Vector4S vector)
		{
			return new Vector2S(vector);
		}

		public static implicit operator Vector4S(Vector2S vector)
		{
			return new Vector4S(vector);
		}
	}
	
	[Serializable]
	public struct Vector3S
	{
		public float x;
		public float y;
		public float z;

		public Vector3S(float x)
		{
			this.x = x;
			this.y = 0f;
			this.z = 0f;
		}

		public Vector3S(float x, float y)
		{
			this.x = x;
			this.y = y;
			this.z = 0f;
		}

		public Vector3S(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vector3S(Vector2 vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = 0f;
		}

		public Vector3S(Vector3 vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
		}

		public Vector3S(Vector4 vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
		}

		public Vector3S(Vector2S vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = 0f;
		}

		public Vector3S(Vector3S vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
		}

		public Vector3S(Vector4S vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
		}

		public static implicit operator Vector3S( Vector2 vector)
		{
			return new Vector3S(vector);
		}

		public static implicit operator Vector2( Vector3S vector)
		{
			return new Vector2(vector.x, vector.y);
		}

		public static implicit operator Vector3S( Vector3 vector)
		{
			return new Vector3S(vector);
		}

		public static implicit operator Vector3( Vector3S vector)
		{
			return new Vector3(vector.x, vector.y, vector.z);
		}

		public static implicit operator Vector3S(Vector4 vector)
		{
			return new Vector3S(vector);
		}

		public static implicit operator Vector4(Vector3S vector)
		{
			return new Vector4(vector.x, vector.y, vector.z);
		}

		public static implicit operator Vector3S(Vector2S vector)
		{
			return new Vector3S(vector);
		}

		public static implicit operator Vector2S(Vector3S vector)
		{
			return new Vector2S(vector);
		}

		public static implicit operator Vector3S(Vector4S vector)
		{
			return new Vector3S(vector);
		}

		public static implicit operator Vector4S(Vector3S vector)
		{
			return new Vector4S(vector);
		}
	}
	
	[Serializable]
	public struct Vector4S
	{

		public float x;
		public float y;
		public float z;
		public float w;

		public Vector4S(float x)
		{
			this.x = x;
			this.y = 0f;
			this.z = 0f;
			this.w = 0f;
		}

		public Vector4S(float x, float y)
		{
			this.x = x;
			this.y = y;
			this.z = 0f;
			this.w = 0f;
		}

		public Vector4S(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = 0f;
		}

		public Vector4S(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public Vector4S(Vector2 vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = 0f;
			this.w = 0f;
		}

		public Vector4S(Vector3 vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
			this.w = 0f;
		}

		public Vector4S(Vector4 vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
			this.w = vector.w;
		}

		public Vector4S(Vector2S vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = 0f;
			this.w = 0f;
		}

		public Vector4S(Vector3S vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
			this.w = 0f;
		}

		public Vector4S(Vector4S vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
			this.w = vector.w;
		}

		public static implicit operator Vector4S(Vector2 vector)
		{
			return new Vector4S(vector);
		}

		public static implicit operator Vector2(Vector4S vector)
		{
			return new Vector2(vector.x, vector.y);
		}

		public static implicit operator Vector4S(Vector3 vector)
		{
			return new Vector4S(vector);
		}

		public static implicit operator Vector3(Vector4S vector)
		{
			return new Vector3(vector.x, vector.y, vector.z);
		}

		public static implicit operator Vector4S(Vector4 vector)
		{
			return new Vector4S(vector);
		}

		public static implicit operator Vector4(Vector4S vector)
		{
			return new Vector4(vector.x, vector.y, vector.z);
		}

		public static implicit operator Vector4S(Vector2S vector)
		{
			return new Vector4S(vector);
		}

		public static implicit operator Vector2S(Vector4S vector)
		{
			return new Vector2S(vector);
		}

		public static implicit operator Vector4S(Vector3S vector)
		{
			return new Vector4S(vector);
		}

		public static implicit operator Vector3S(Vector4S vector)
		{
			return new Vector3S(vector);
		}
	}
	
	[Serializable]
	public class MeshS
	{
		public Vector3S[] vertices;
		public int[] triangles;
		public Vector2S[] uv;
		public Vector3S[] normals;
		public Color[] colors;
		public Color32[] colors32;

		public MeshS(Mesh mesh)
		{
			vertices = mesh.vertices.Cast<Vector3S>().ToArray ();
			triangles = mesh.triangles;
			uv = mesh.uv.Cast<Vector2S>().ToArray();
			normals = mesh.normals.Cast<Vector3S>().ToArray();
			colors = mesh.colors.Cast<Color>().ToArray();
			colors32 = mesh.colors32.Cast<Color32>().ToArray();
		}

		public static implicit operator MeshS(Mesh mesh)
		{
			return new MeshS(mesh);
		}

		public static implicit operator Mesh(MeshS mesh)
		{
			Mesh newMesh = new Mesh();
			newMesh.vertices = mesh.vertices.Cast<Vector3>().ToArray ();
			newMesh.triangles = mesh.triangles;
			newMesh.normals = mesh.normals.Cast<Vector3>().ToArray ();
			newMesh.uv = mesh.uv.Cast<Vector2>().ToArray ();
			newMesh.colors = mesh.colors.Cast<Color>().ToArray();
			newMesh.colors32 = mesh.colors32.Cast<Color32>().ToArray();
			
			return newMesh;
		}
	}
	
	[Serializable]
	public struct QuaternionS
	{
		public float x;
		public float y;
		public float z;
		public float w;

		public QuaternionS(float x)
		{
			this.x = x;
			this.y = 0f;
			this.z = 0f;
			this.w = 0f;
		}

		public QuaternionS(float x, float y)
		{
			this.x = x;
			this.y = y;
			this.z = 0f;
			this.w = 0f;
		}

		public QuaternionS(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = 0f;
		}

		public QuaternionS(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public QuaternionS(Quaternion quaternion)
		{
			this.x = quaternion.x;
			this.y = quaternion.y;
			this.z = quaternion.z;
			this.w = quaternion.w;
		}

		public static implicit operator QuaternionS(Quaternion quaternion)
		{
			return new QuaternionS(quaternion);
		}

		public static implicit operator Quaternion(QuaternionS quaternion)
		{
			return new Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
		}
	}
}