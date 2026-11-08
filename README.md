# Texture Settings Copier

Unity エディタ拡張です。2つのフォルダ内で**同じファイル名のテクスチャ**を探し、コピー元のインポート設定（Max Size、圧縮形式、Mip Map、Filter/Wrap Mode など）をコピー先へ一括で適用します。

## 導入方法

`Assets/Editor/TextureSettingsCopier.cs` を、ご自身の Unity プロジェクトの任意の `Editor` フォルダに配置してください。

## 使い方

1. メニューの **Tools > yarihcas1_lab > Texture Settings Copier** を開きます。
2. 「コピー元フォルダ」に設定の取得元、「コピー先フォルダ」に適用先のフォルダを指定します。
3. **プレビュー** で、マッチしたテクスチャと変更される設定を確認します。
4. **設定を一括適用** で、変更のあるテクスチャに設定を適用します。

## コピーされる設定

- Texture Type / Shape、sRGB、Alpha Source、Alpha Is Transparency
- Non-Power of 2、Read/Write、Streaming Mipmaps
- Mip Map 関連（Generate Mip Maps、Border、Filter、Fadeout）
- Wrap Mode（U/V/W）、Filter Mode、Aniso Level
- Max Size、Compression、Compression Quality、Crunch Compression
- プラットフォーム別設定（Standalone / Android / iPhone / WebGL のうち、上書きされているもの）
- Sprite 設定（Sprite Mode、Pixels Per Unit、Pivot、Border）※ Texture Type が Sprite の場合

## 注意

- サブフォルダも含めて検索し、ファイル名（拡張子込み）で照合します。同じフォルダツリー内に同名ファイルが複数ある場合は、1つだけが対象になります。
- 適用前にプロジェクトのバックアップ、またはバージョン管理でのコミットをおすすめします。

## ライセンス

[MIT License](LICENSE)
