# 商城服务和 API

## 开发

```
dotnet watch run
```

## 部署

1. 在项目的 ShopAPI 文件夹下运行以下命令，进行：

- 打包
- 将打包好的项目上传到服务器（期间需要输入服务器登录密码）

```shell
bash ./scripts/publish.sh
```

2. 在服务器中运行以下命令重启服务：

```shell
cd /var/www && rm -rf shop-api/* && mv shop-api.tgz shop-api && cd shop-api && tar -zxvf shop-api.tgz && sudo systemctl restart kestrel-shop-api.service
```
