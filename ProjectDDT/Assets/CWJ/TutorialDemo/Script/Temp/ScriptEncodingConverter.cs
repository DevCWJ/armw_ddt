#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace CWJ.EditorOnly
{
	// .editorconfig 파일만들어서 Asset폴더에 두고 내용을
	// # All files
	// [*]
	// end_of_line = crlf
	// insert_final_newline = true
	// charset = utf-8 # <======= 추가
	//
	// # Xml files
	// [*.xml]
	// indent_size = 2
	// 로 하면 끝줄 경고 해결됨
	public class ScriptEncodingConverter : EditorWindow
	{
		private string folderPath = "";

		[MenuItem("CWJ/Tools/Script Encoding Convert")]
		public static void ShowWindow()
		{
			GetWindow(typeof(ScriptEncodingConverter), false, "Convert Encoding");
		}

		void OnGUI()
		{
			GUILayout.Label("Encoding 변환 도구\nUTF8 > EUC-KR > UTF8+BOM 변환으로 깨지지않게 저장합니다", EditorStyles.boldLabel);

			if (GUILayout.Button("폴더 선택"))
			{
				folderPath = EditorUtility.OpenFolderPanel("변환할 폴더 선택", Application.dataPath, "");
			}

			GUILayout.Label("선택된 폴더: " + folderPath);

			if (!string.IsNullOrEmpty(folderPath))
			{
				if (GUILayout.Button("cs 파일 인코딩 변환"))
				{
					ConvertAllCsFiles(folderPath);
				}
			}
		}

		void ConvertAllCsFiles(string path)
		{
			// 선택한 폴더 내 모든 .cs 파일 검색 (하위 폴더 포함)
			string[] files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
			int count = 0;

			foreach (var file in files)
			{
				try
				{
					// EUC-KR 인코딩으로 파일 읽기
					string content = File.ReadAllText(file, Encoding.GetEncoding("euc-kr"));

					// UTF-8 BOM 포함 인코딩 으로 파일 쓰기
					File.WriteAllText(file, content, new UTF8Encoding(true));
					count++;
				}
				catch (Exception ex)
				{
					Debug.LogError("파일 변환 중 오류 발생: " + file + "\n" + ex.Message);
				}
			}

			Debug.Log($"총 {count}개의 파일을 UTF-8 BOM으로 변환했습니다.\n" + folderPath);
		}
	}
}

#endif
