// 
// MacApis.cs: Mac APIs for System.IO
//
// 
// Authors:
//   Miguel de Icaza (miguel@microsoft.com)
//
// Copyright (C) 2018 Microsoft Corp
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//
// The FULL macro can be used to surface a larger API
//
using System.Runtime.InteropServices;
using System;
using copy_file_state_t=System.IntPtr;

namespace Darwin {

	/// <summary>
	/// Surfaces the Darwin CopyFile API which can both copy individual files
	/// as well as copying directory structures.
	/// </summary>
	public class CopyFile {
		
		[Flags]
		/// <summary>
		///   Flags that control how the CopyFile APIs operate
		/// </summary>
		public enum Flags {
			/// <summary>
			/// Copy the source file's access control lists.
			/// </summary>
			Acl = 1 << 0,
			/// <summary>
			/// Copy the source file's POSIX information (mode, modification time, etc.).
			/// </summary>
			Stat = 1 << 1,
			/// <summary>
			/// Copy the source file's extended attributes.
			/// </summary>
			Xattr = 1 << 2,
			/// <summary>
			/// Copy the source file's data.
			/// </summary>
			Data = 1 << 3,
			/// <summary>
			/// Copy the source file's POSIX and ACL information; equivalent to Stat | Acl
			/// </summary>
			Security = Stat | Acl,
			/// <summary>
			/// Copy the metadata; equivalent to Security | Xattr
			/// </summary>
			Metadata = Security | Xattr,
			/// <summary>
			/// Copy the entire file; equivalent to Metadata | All
			/// </summary>
			All = Metadata | Data,

			/// <summary>
			/// Causes CopyFile  to recursively copy a hierarchy.  It is not used by FCopyFile.
			/// </summary>
			Recursive = 1 << 15,
			/// <summary>
			/// Return a bitmask, corresponding to the Flags indicating which contents would be copied; no
                        /// data are actually copied.  (E.g., if flags was set to Check|Metadata, and the from
                        /// file had extended attributes but no ACLs, the return value would be Xattr)
			/// </summary>
			Check = 1 << 16,
			/// <summary>
			/// Fail if the to file already exists. 
			/// </summary>
			Excl = 1 << 17,
			/// <summary>
			/// Do not follow the from file, if it is a symbolic link (only CopyFile).
			/// </summary>
			NoFollowSrc = 1 << 18,
			/// <summary>
			/// Do not follow the to file, if it is a symbolic link (only CopyFile)
			/// </summary>
			NoFollowDst = 1 << 19,
			/// <summary>
			/// Unlink (using remove(3)) the from file.  (Only CopyFile)  No
                        /// error is returned if remove(3) fails.  Note that remove(3) removes a symbolic link itself, not the
			/// target of the link.
			/// </summary>
			Move = 1 << 20,
			/// <summary>
			///  Unlink the to file before starting.  (Only CopyFile).
			/// </summary>
			Unlink = 1 << 21,
			/// <summary>
			/// Equivalent to NoFollowDst and NoFollowSrc
			/// </summary>
			NoFollow = 1 << NoFollowSrc | NoFollowDst,
	
			/// <summary>
			/// Serialize the from file.  The to file is an AppleDouble-format file.
			/// </summary>
			Pack = 1 << 22,
			/// <summary>
			///Unserialize the from file.  The from file is an AppleDouble-format file; the to file will have the
			/// extended attributes, ACLs, resource fork, and FinderInfo data from the to file, regardless of the
                        /// flags argument passed in.
			/// </summary>
			UnPack = 1 << 23,
			/// <summary>
			///  Try to clone the file instead.  This is a best try flag i.e. if cloning fails, fallback to copying the
                        ///  file.  This flag is equivalent to Excl | Acl | Stat | Xattr | Data | NoFollowSrc
                        ///  Note that if cloning is successful, progress callbacks will
                        ///  not be invoked.  Note also that there is no support for cloning directories: if a directory is pro-
                        ///  vided as the source and CloneForce is not passed, this will instead copy the directory.
                        ///  Recursive copying however is supported, see below for more information.  (Only CopyFile).
			/// </summary>
			Clone = 1 << 24,
			/// <summary>
			/// Clone the file instead.  This is a force flag i.e. if cloning fails, an error is returned.  This flag
                        /// is equivalent to Excl | Acl | Stat | Xattr | Data | NoFollowSrc
                        /// Note that if cloning is successful, progress callbacks will not be invoked.
                        /// Note also that there is no support for cloning directories: if a directory is provided as the source,
                        /// an error will be returned.  (Only CopyFile).
			/// </summary>
			CloneForce = 1 << 25,
			/// <summary>
			/// If the src file has quarantine information, add the QTN_FLAG_DO_NOT_TRANSLOCATE flag to the quarantine
                        /// information of the dst file. This allows a bundle to run in place instead of being translocated.
			/// </summary>
			RunInPlace = 1 << 26,
			/// <summary>
			/// Copy a file sparsely.  This requires that the source and destination file systems support sparse files
                        /// with hole sizes at least as large as their block sizes.  This also requires that the source file is
                        /// sparse, and for fcopyfile() the source file descriptor's offset be a multiple of the minimum hole
                        /// size.  If Data is also specified, this will fall back to a full copy if sparse copying cannot
                        /// be performed for any reason; otherwise, an error is returned.
			/// </summary>
			DataSparse = 1 << 27,
			/// <summary>
			///
			/// </summary>
			Verbose = 1 << 30,
		}

		/// <summary>
		///  Provides the progress of a recursive copy operation
		/// </summary>
		public enum Progress {
			/// <summary>
			/// There was an error in processing an element of the source hierarchy; this happens when fts(3)
                        /// returns an error or unknown file type.  (Currently, the second argument to the call-back 
                        /// function will always be Progress.Error in this case.)
			/// </summary>
			Error = 0,
			/// <summary>
			/// The object being copied is a file (or, rather, something other than a directory).
			/// </summary>
			File = 1,
			/// <summary>
			/// The object being copied is a directory, and is being entered.  (That is, none of the filesystem
                        /// objects contained within the directory have been copied yet.)
			/// </summary>
			Directory = 2,
			/// <summary>
			/// The object being copied is a directory, and all of the objects contained have been copied.  At
                        /// this stage, the destination directory being copied will have any extra permissions that were
			/// added to allow the copying will be removed.
			/// </summary>
			DirectoryCleanup = 3,
			/// <summary>
			/// Invoked on every write call.   The second argument will either be Stage.Progress, or Stage.Error.
			/// The amount of data copied so far can be retrieved from the State.Copied property.
			/// </summary>
			CopyData = 4,
			/// <summary>
			/// Invoked when copying is copying extended attributes.  The name of the extended attribute being
			/// copied will be on the State.XattrName property.  Any attribute skipped by returning Skip from the
			// Start callback will not be placed into the  packed output file.
			/// </summary>
			CopyXattr = 5,
		}

		public enum Stage {
			Start = 1,
			Finish = 2,
			Error = 3,
			Progress = 4
		}

		/// <summary>
		/// Possible return values from the State callback method.
		/// </summary>
		public enum NextStep {
			/// <summary>
			/// Continue copying
			/// </summary>
			Continue = 0,
			/// <summary>
			/// Skip this file
			/// </summary>
			Skip = 1,
			/// <summary>
			/// Stop copying
			/// </summary>
			Quit = 2
		}
	
		[DllImport ("libc")]
		public extern static copy_file_state_t copyfile_state_alloc ();
		
		[DllImport ("libc", SetLastError=true)]
		public extern static int copyfile_state_free (copy_file_state_t buf);

		[DllImport ("libc")]
		public extern static int fcopyfile (int fromfd, int tofd, copy_file_state_t state, Flags flags);

		[DllImport ("libc", SetLastError=true)]
		public extern static int copyfile (string from, string to, copy_file_state_t state, Flags flags);

		/// <summary>
		///   State that can be used to call CopyFile, you can use this to configure some properties of the
		///   copy operation.
		/// </summary>
		/// <remarks>
		///   <para>Use the SetStatusCallback method to configure a callback method to be used for recursive copy operations.</para>
		/// </remarks>
		public class State : IDisposable {
			internal IntPtr handle;

			public State()
			{
				handle = copyfile_state_alloc ();
			}

			void IDisposable.Dispose ()
			{
				Dispose (true);
				GC.SuppressFinalize (this);
			}

			~State ()
			{
				Dispose (false);
			}

			protected virtual void Dispose (bool disposing)
			{
				if (handle != IntPtr.Zero){
					copyfile_state_free (handle);
					handle = IntPtr.Zero;
				}
				if (gch.IsAllocated){
					gch.Free ();
				}
			}

			enum Option {
				SrcFD, // surfaced
				SrcFileName, // surfaced
				DstFD, // surfaced
				DstFileName, // surfaced
				Quarantine,
				StatusCB, // surfaced
				StatusCtx, // surfaced
				Copied, //surfaced
				XAttrName, // surfaced
				WasCloned
			}

			[DllImport ("libc")]
			extern static int copyfile_state_get (copy_file_state_t s, Option flag, ref long dst);

			[DllImport ("libc")]
			extern static int copyfile_state_get (copy_file_state_t s, Option flag, ref int dst);

			[DllImport ("libc")]
			extern static int copyfile_state_set (copy_file_state_t s, Option flag, ref int src);

			[DllImport ("libc")]
			extern static int copyfile_state_set (copy_file_state_t s, Option flag, ref IntPtr src);

			[DllImport ("libc")]
			extern static int copyfile_state_get (copy_file_state_t s, Option flag, ref IntPtr v);

			[DllImport ("libc")]
			extern static int copyfile_state_set (copy_file_state_t s, Option flag, string v);

			delegate NextStep RawProgressCallback (int what, int stage, IntPtr state, IntPtr str, IntPtr dst, IntPtr ctx);
			
			[DllImport ("libc")]
			extern static int copyfile_state_set (copy_file_state_t s, Option flag, RawProgressCallback callback);

			delegate int copyfile_callback_t (int what, int stage, copy_file_state_t state, string src, string dst, IntPtr ctx);

			/// <summary>
			/// Get or set the file descriptor associated with the source file.
			/// If this has not been initialized yet, the value will be -2.
			/// </summary>
			public int SourceFD {
				get {
					int fd = 0;
					copyfile_state_get (handle, Option.SrcFD, ref fd);
					return fd;
				}
				set {
					var fd = value;
					copyfile_state_set (handle, Option.SrcFD, ref fd);
				}
			}

			/// <summary>
			/// Get or set the file descriptor associated with the destiation file.
			/// If this has not been initialized yet, the value will be -2.
			/// </summary>
			public int DestFD {
				get {
					int fd = 0;
					copyfile_state_get (handle, Option.DstFD, ref fd);
					return fd;
				}
				set {
					var fd = value;
					copyfile_state_set (handle, Option.DstFD, ref fd);
				}
			}

			/// <summary>
			/// Get or set the filename associated with the source, if this has not been
			/// initialized, the value will be null.
			/// </summary>.
			public string SourceFileName {
				get {
					IntPtr x = IntPtr.Zero;
					copyfile_state_get (handle, Option.SrcFileName, ref x);
					return Marshal.PtrToStringAnsi (x);
				}
				set {
					copyfile_state_set (handle, Option.SrcFileName, value);
				}
			}

			/// <summary>
			/// The number of bytes copied so far
			/// </summary>
			public long Copied {
				get {
					long x = 0;
					copyfile_state_get (handle, Option.Copied, ref x);
					return x;
					
				}
			}

			/// <summary>
			/// Get or set the filename associated with the source, if this has not been
			/// initialized, the value will be null.
			/// </summary>.
			public string DestinationFileName {
				get {
					IntPtr x = IntPtr.Zero;
					copyfile_state_get (handle, Option.DstFileName, ref x);
					return Marshal.PtrToStringAnsi (x);
				}
				set {
					copyfile_state_set (handle, Option.DstFileName, value);
				}
			}

			/// <summary>
			/// Retursn the name of the current extended attribute being copied
			/// </summary>
			public string XAttrName {
				get {
					IntPtr x = IntPtr.Zero;
					copyfile_state_get (handle, Option.XAttrName, ref x);
					return Marshal.PtrToStringAnsi (x);
				}
			}

			public delegate NextStep ProgressCallback (Progress what, Stage stage, string source, string dest, State state);
			
			static NextStep Callback (int what, int stage, IntPtr state, IntPtr src, IntPtr dst, IntPtr ctx)
			{
				var gch = GCHandle.FromIntPtr (ctx);
				State s = gch.Target as State;
				return s.callback ((Progress) what, (Stage)stage, Marshal.PtrToStringAnsi (src), Marshal.PtrToStringAnsi (dst), s);
			}

			ProgressCallback callback;
			GCHandle gch;

			/// <summary>
			/// Sets a callback that is invoked during recursive copies and can allow you to
			/// monitor and control the copying process.
			/// </summary>
			public void SetStatusCallback (ProgressCallback callback)
			{
				this.callback = callback;
				if (!gch.IsAllocated)
					gch = GCHandle.Alloc (this);
				
				copyfile_state_set (handle, Option.StatusCB, Callback);
				IntPtr h = GCHandle.ToIntPtr (gch);
				copyfile_state_set (handle, Option.StatusCtx, ref h);
			}
		}

		/// <summary>
		///   Possible return values from perfoming the copy operation
		/// </summary>
		public enum Status {
			Ok = 0,
			EINVAL = 22,
			ENOTMEM = 12,
			ENOTSUP = 45,
			ECANCELED = 89,
			EEXISTS = 17,
			ENOENT = 2,
			EACCESS = 13
		}

		/// <summary>
		/// This function copies a file or directory to a destination.
		/// </summary>
		/// <remarks>
		///   <para>
		///     The 
		///   </para>
		///   <para>
		///   </para>
		///   <para>
		///     The copy operation can be configured using the flags parameter.
		///   </para>
		///   <para>
		///     To get control over the copy operation during recursive copies, you can provide a State object.
		///   </para>
		/// </remarks>
		public static Status Run(string from, string to, Flags flags, State state = null)
		{
			if (from == null)
				throw new ArgumentNullException (nameof (from));
			if (to == null)
				throw new ArgumentNullException (nameof (to));
			
			var ret = copyfile (from, to, state == null ? IntPtr.Zero : state.handle, flags);
			if (ret != 0)
				return (Status) Marshal.GetLastWin32Error ();
			return Status.Ok;
		}
	}
}
