#!/bin/bash

set -e

pwd

dir=$(pwd)

tgz_name="shop-api.tgz"

# 打包
dotnet publish --configuration Release

cd bin/Release/netcoreapp3.1/publish

# 压缩
tar -zcvf "../../../../${tgz_name}" .

# 上传到服务器
echo "开始上传文件，请输入服务器密码："
# 上传文件
scp "${dir}/${tgz_name}" root@118.178.123.78:/var/www/

echo "文件上传成功！"
