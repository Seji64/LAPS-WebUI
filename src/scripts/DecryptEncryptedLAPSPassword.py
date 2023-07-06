import dpapi_ng
import base64
import argparse

def main():
     parser = argparse.ArgumentParser(prog='LAPS-WebUIPythonExt')
     parser.add_argument('-U','--user', default=None, required=True, dest='username')
     parser.add_argument('-P','--password', default=None, required=True, dest='password')
     parser.add_argument('-d','--data', default=None, required=True, dest='base64payload')
    
     args = parser.parse_args()
     encyrptedPass = base64.b64decode(args.base64payload)
     decryptedBlob = dpapi_ng.ncrypt_unprotect_secret(username=args.username, password=args.password, data=encyrptedPass)
     print(str(decryptedBlob,'utf-16'))

if __name__ == '__main__':
	main()