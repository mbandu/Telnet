# Telnet
C# telnet client and server with RFC2217 option.



# Requirement
It's normal that you get this error.

![Serial list](./serial3.png)

The nuget package do not carry the necessary Linux serial library. You'll basically will need to compile them. It is documented on the GitHub page. In short:
```CMD
git clone https://github.com/jcurl/serialportstream.git
cd serialportstream/
cd dll/serialunix/
./build.sh
```



# Example
