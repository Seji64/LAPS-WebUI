import dpapi_ng
import base64
import argparse
import sys

def main():
     parser = argparse.ArgumentParser(prog='LAPS-WebUIPythonExt')
     parser.add_argument('-d','--data', default=None, required=True, dest='base64payload')

     args = parser.parse_args()
     username = sys.stdin.readline().rstrip('\n')
     password = sys.stdin.readline().rstrip('\n')
     encyrptedPass = base64.b64decode(args.base64payload)
     decryptedBlob = dpapi_ng.ncrypt_unprotect_secret(username=username, password=password, data=encyrptedPass)
     print(str(decryptedBlob,'utf-16'))

if __name__ == '__main__':
	main()