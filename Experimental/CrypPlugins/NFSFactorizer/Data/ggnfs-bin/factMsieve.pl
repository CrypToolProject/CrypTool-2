#!/usr/bin/perl
# The path where the binaries are:
$GGNFS_BIN_PATH="../../ggnfs/bin";
# And some other popular choices:
#$GGNFS_BIN_PATH="../../src";
#$GGNFS_BIN_PATH="../ggnfs.vc/bin";
#$GGNFS_BIN_PATH="c:/mingw/msys/1.0/home/SamAdmin/ggnfs-0.77.1/src";

# This is a conversion of factLat.pl to work with msieve.
# Simply place the msieve binary in the GGNFS bin directory, and use
# this script.
#
# Msieve is now used to do the classical sieve.
#
# Parameter changes won't work at this time.  Nor is all of the info 
# about the factorization correct in the summary file.  These are on my 
# todo list.  But I wanted to get this out since I may not have time to 
# work on it again for a short while.  
#
# I have added the ability to launch multiple lattice sieves
# on machines with more than one CPU core.  Set NUM_CPUS below to 
# use this.  There are a couple of caveats with this, however.  Perl
# complains about an attempt to free an unreferenced scalar, but continues
# on.  Also, trying to exit with Ctrl-C doesn't seem to work correctly.

########################################################################
# factLat.pl
# Copyright 2004, Chris Monico.
#
#   This file is part of GGNFS.
#   GGNFS is free software; you can redistribute it and/or modify
#   it under the terms of the GNU General Public License as published by
#   the Free Software Foundation; either version 2 of the License, or
#   (at your option) any later version.
#
#   GGNFS is distributed in the hope that it will be useful,
#   but WITHOUT ANY WARRANTY; without even the implied warranty of
#   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#   GNU General Public License for more details.
#
#   You should have received a copy of the GNU General Public License
#   along with GGNFS; if not, write to the Free Software
#   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
########################################################################
#  This script is known to work with Perl 5.8, and is known not to
#  work with 5.2. In between - I don't know.
########################################################################

use Math::BigInt;
use Math::BigFloat;
# Bah, this causes a fatal error if GMP BigInt is not available.
# use Math::BigInt lib => 'GMP';

$NUM_CPUS=1;
$NUM_THREADS=$NUM_CPUS;

if ($NUM_THREADS != 1) {
  use Config; 
  if (!($Config{usethreads})) { 
    printf "This version of Perl doesn't support multiple threads.  Only using 1 thread.";
    $NUM_THREADS=1; 
  }
  else {
    use Thread qw(async);
  }
}

# $SYS_BIN_PATH="c:/mingw/msys/1.0/bin";
$SYS_BIN_PATH="";
# $SYS_BIN_PATH="/bin";

$NO_DEF_NM_PARAM=0;
$CLEANUP=0;
$PROMPTS=0;
$DOCLASSICAL=0;
$CHECK_BINARIES=1;
$ECHO_CMDLINE=1;
$CHECK_POLY=1;
# If this is zero, the lattice siever will sieve q-values on the algebraic side.
# Otherwise, it will sieve rational q-values.
$LATSIEVE_SIDE=1;
# SB: 1 (rational) for SNFS, 0 (algebraic) for GNFS!!

# If this is zero, the GGNFS polynomial selection code will be used (when needed).
# But the Kleinjung/Franke code is better, so you should use it if you can.
# i.e., try it - but if you seem to have some fatal errors, you can just change
# this to zero to revert to the GGNFS code.
$USE_KLEINJUNG_FRANKE_PS=1;

# Run the binaries at low priority?
# $NICE="nice -n 19 ";

# Set to 2 if you run into problems (which is definitely possible).
$LARGEP=3;

# This is for an Athlon 2800+ laptop. If your machine is about half as fast,
# replace this with a 2. 25% as fast, replace with a 4. It controls how long
# the polynomial selection phase will last.
$polySelTimeMultiplier=1.0;

################################################################
# Nothing configurable below here - don't mess with it unless  #
# you're fixing a bug or adding functionality.                 #
################################################################

if ($^O eq "MSWin32") {
  $CAT=$SYS_BIN_PATH."/cat.exe";
  $GZIP=$SYS_BIN_PATH."/gzip.exe";
  $EXEC_SUFFIX=".exe";
}
else {
  $CAT="cat";
  $GZIP="gzip";
  $EXEC_SUFFIX="";
}

sub concat {			# for CAT-less people
  ($from, $add, $to) = @_;
  if($CAT) {
    system("\"$CAT\" $from $add $to");
  } else {
    open FFROM, "<$from";
    open FTO, "$add$to";
    while(<FFROM>) {
      print FTO $_;
    }
    close FFROM;
    close FTO;
  }
}

$LATSIEVER_L1=$GGNFS_BIN_PATH."/gnfs-lasieve4I11e".$EXEC_SUFFIX;
$LATSIEVER_L2=$GGNFS_BIN_PATH."/gnfs-lasieve4I12e".$EXEC_SUFFIX;
$LATSIEVER_L3=$GGNFS_BIN_PATH."/gnfs-lasieve4I13e".$EXEC_SUFFIX;
$LATSIEVER_L4=$GGNFS_BIN_PATH."/gnfs-lasieve4I14e".$EXEC_SUFFIX;
$LATSIEVER_L5=$GGNFS_BIN_PATH."/gnfs-lasieve4I15e".$EXEC_SUFFIX;
$LATSIEVER_L6=$GGNFS_BIN_PATH."/gnfs-lasieve4I16e".$EXEC_SUFFIX;
$LATSIEVER=$LATSIEVER_L1; # Just a default.

####################################################################
$MAKEFB=$GGNFS_BIN_PATH."/makefb".$EXEC_SUFFIX;
$PROCRELS=$GGNFS_BIN_PATH."/procrels".$EXEC_SUFFIX;
$MSIEVE=$GGNFS_BIN_PATH."/msieve".$EXEC_SUFFIX;
$POLYSELECT=$GGNFS_BIN_PATH."/polyselect".$EXEC_SUFFIX;
$POL51M0=$GGNFS_BIN_PATH."/pol51m0b".$EXEC_SUFFIX;
$POL51OPT=$GGNFS_BIN_PATH."/pol51opt".$EXEC_SUFFIX;
$PLOT=$GGNFS_BIN_PATH."/autogplot.sh";
$DEFAULT_PAR_FILE=$GGNFS_BIN_PATH."/def-par.txt";
$DEFAULT_POLSEL_PAR_FILE=$GGNFS_BIN_PATH."/def-nm-params.txt";

$RELSBIN="rels.bin";
$LOGFILE="ggnfs.log";
$LARGEPRIMES="-".$LARGEP."p";

# This file is used by this script to detect
# changes in parameter settings.
$PARAMFILE=".params";

# This is to tell the lattice siever where to dump the next special-q
# if it's interrupted, for example, with CTRL-C.
$PNUM=0;
                                                                                                
###########################################################
sub gcd {
###########################################################
# This will be used to make a function which peels off prime factors
# when the square root step returns composite factors, so that
# (1) We can report the prime factors themselves.
# (2) We know when enough square roots have been computed.
###########################################################
# Sample usage: (Note that the declarations must be this way,
# to force the variables to have the right type!).
#$x= Math::BigInt->new($ARGV[0]);
#$y= Math::BigInt->new($ARGV[1]);
#$g=gcd($x, $y);
###########################################################
  if ($_[0]==0) { return $_[1]; }
  if ($_[1]==0) { return $_[0]; }
  return gcd($_[1]%$_[0], $_[0]);
}

#-------------------------------------------------------------------------------
#probab_prime_p(n, reps)
#  Return whether n is probably prime or not.
#    '' = composite
#    1 = probably prime
#    2 = prime
#  This function uses GMP PERL MODULE (gmp-4.1.4/demos/perl) if available.
#  GMP::Mpz::probab_prime_p is very fast but never returns 2 because it's a bool
#  function.
if (eval('use GMP::Mpz;1;')) {
  *probab_prime_p = sub {
    GMP::Mpz::probab_prime_p("$_[0]", $_[1]);  #convert Math::BigInt to scalar
  };
} else {
  *probab_prime_p = sub {
    my ($n, $reps) = @_;
    ref $n or $n = new Math::BigInt($n);
    $n->is_negative() and $n->babs();
    #Trial division
    if ($n < 1000000) {
      $n = "$n" + 0;  #scalar
      $n < 2 and return '';
      foreach (@primes1000) {
        my $q = int($n / $_);
        $q < $_ and return 2;
        $n - $_ * $q or return '';
      }
      return 2;
    }
    $n->bgcd($primorial168)->is_one() or return '';  #bgcd returns new object
    #Fermat test
    #  gcd(n,210)==1 is ensured by trial division.
    my $nm1 = $n->copy()->bdec();  #new object
    new Math::BigInt(210)->bmodpow($nm1, $n)->is_one() or return '';
    #Miller Rabin test
    #  Reference: gmp-4.1.4/mpz/millerrabin.c
    my $k = scan1($nm1, 0);
    my $q = $nm1->copy()->brsft($k, 2);  #new object
  MILLER_RABIN_LOOP:
    while ($reps-- > 0) {
      my $x;
      do {
        $x = urandomb(sizeinbase($n, 2) - 1);
      } while ($x <= 1);
      $x->bmodpow($q, $n)->is_one() || $x == $nm1 and next MILLER_RABIN_LOOP;
      my $i;
      for ($i = 1; $i < $k; $i++) {
        $x->bmul($x)->bmod($n) == $nm1 and next MILLER_RABIN_LOOP;
        $x->is_one() and return '';
      }
      return '';
    }
    1;
  };
  #primes1000
  #  Primes up to 1000.
  @primes1000 = (
    2, 3, 5, 7, 11, 13, 17, 19, 23, 29,
    31, 37, 41, 43, 47, 53, 59, 61, 67, 71,
    73, 79, 83, 89, 97, 101, 103, 107, 109, 113,
    127, 131, 137, 139, 149, 151, 157, 163, 167, 173,
    179, 181, 191, 193, 197, 199, 211, 223, 227, 229,
    233, 239, 241, 251, 257, 263, 269, 271, 277, 281,
    283, 293, 307, 311, 313, 317, 331, 337, 347, 349,
    353, 359, 367, 373, 379, 383, 389, 397, 401, 409,
    419, 421, 431, 433, 439, 443, 449, 457, 461, 463,
    467, 479, 487, 491, 499, 503, 509, 521, 523, 541,
    547, 557, 563, 569, 571, 577, 587, 593, 599, 601,
    607, 613, 617, 619, 631, 641, 643, 647, 653, 659,
    661, 673, 677, 683, 691, 701, 709, 719, 727, 733,
    739, 743, 751, 757, 761, 769, 773, 787, 797, 809,
    811, 821, 823, 827, 829, 839, 853, 857, 859, 863,
    877, 881, 883, 887, 907, 911, 919, 929, 937, 941,
    947, 953, 967, 971, 977, 983, 991, 997,
  );
  #primorial168
  #  Product of primes up to 1000.
  $primorial168 = Math::BigInt->bone();
  foreach (@primes1000) {
    $primorial168->bmul($_);
  }
  #scan1(n, start)
  #  Return the index of the least significant 1 in base 2.
  *scan1 = sub {
    my ($n, $start) = @_;
    $n = new Math::BigInt($n);  #new object
    $n->is_negative() and $n->babs();
    $start > 0 and $n->brsft($start, 2);
    $n->is_zero() and return 0x7fffffff;
    my $q;
    ($n, $q) = $n->bdiv(0x40000000);
    until ($q) {
      $start += 30;
      ($n, $q) = $n->bdiv(0x40000000);
    }
    until ($q & 1) {
      $start++;
      $q >>= 1;
    }
    $start;
  };
  #sizeinbase(n, base)
  #  Return the size of n measured in number of digits in base base.
  *sizeinbase = sub {
    my ($n, $base) = @_;
    $n = new Math::BigInt($n);  #new object
    $n->is_negative() and $n->babs();
    $base < 2 and $base = 2;
    $base = new Math::BigInt($base);  #new object
    my @list = ();
    while ($base <= $n) {
      push(@list, $base);
      $base *= $base;  #new object
    }
    my $size = 1;
    while (@list) {
      $base = pop(@list);
      if ($base <= $n) {
        $n->bdiv($base);
        $size += 1 << @list;
      }
    }
    $size;
  };
  #urandomb(size)
  #  Generate a random integer in the range 0 to 2^size-1, inclusive.
  *urandomb = sub {
    my ($size) = @_;
    my $n = new Math::BigInt(int(rand(1 << ($size % 30))));
    while ($size >= 30) {
      $n->bmul(0x40000000)->bior(int(rand(0x40000000)));  #don't use blsft
      $size -= 30;
    }
    $n;
  };
}
#-------------------------------------------------------------------------------

###########################################################
sub linecount {
###########################################################
# Count the number of lines in the DATFILE
###########################################################
  my $count = 0;
  open(FILE, "< $DATNAME") or die "can't open $DATNAME: $!";
  $count++ while <FILE>;
  return $count;
}

###########################################################
sub getPrimes {
###########################################################
# Read the log file to see if we've found all the prime
# divisors of N yet.
###########################################################

  open(INFO,$LOGFILE);
  while (<INFO>) {
    chomp;
    @_ = split;
    if (/r\d=/) {
      s/.*r\d=//;
      if ((length($_) > 1) && (length($_) < length($NDIVFREE))) {
        # Is this a prime divisor or composite?
        if (/pp/) {
          s/\(.*\)//; # Strip off the (pp <digits>) part.
          s/\s*//g; # Remove whitespace.
          # If this is a prime we don't already have, add it.
          my $found=0;
          for ($i=0; $i<=$#PRIMES; $i++) {
            if ($_ == $PRIMES[$i]) { $found=1; }
          }
          if (!($found)) { push(@PRIMES, $_); }
        } else {
          s/\(.*\)//; # Strip off the (c<digits>) part.
          s/\s*//g; # Remove whitespace.
          push(@COMPS, $_);
        }
      }
    }
  }
  close(INFO);
  # Now, try to figure out if we have all the prime factors:
  my $x= Math::BigInt->new('1');
  for ($i=0; $i<=$#PRIMES; $i++) {
    $x = $x*$PRIMES[$i];
  }
  if ($x==$NDIVFREE || probab_prime_p($NDIVFREE/$x, 10)) { 
    $x==$NDIVFREE or push(@PRIMES, $NDIVFREE/$x);
    open(OF, ">>$LOGFILE");
    while ($_ = shift @PRIMES) {
      printf(OF "-> p: $_ (pp%d)\n", length($_));
    }
    return 1; 
  }
  # Here, we could try to recover other factors by division,
  # but until we have a primality test available, this would
  # be pointless since we couldn't really know if we're done.
  return 0; 
    
}

###########################################################
sub sigDie {
###########################################################
  die "Signal caught. Terminating...\n";
}

my $nonPrefDegAdjust = 12;
######################################################
sub loadDefaultParams {
######################################################
# 
# These are default parameters for different size factorizations.
# They will be used only to fill in any non-user-supplied parameters.
# The format of the file is as follows:
# type, digits, deg, maxs1, maxskew, goodScore, eFrac, j0, j1, eStepSize, maxTime,
#       rlim, alim, lpbr, lpba, mfbr, mfba, rlambda, alambda, qintsize
# where 'type' is gnfs or snfs.
# arg0 = number of digits in N.
# arg1 = type
# 
# The parameter actual_degree, if nonzero, will let this function adjust
# parameters for SNFS factorizations with a predetermined polynomial
# of degree that is not the optimal degree.

  die("loadDefaultParams(): Insufficient arguments!\n") 
      unless ($#_ >= 2);
  my $realDIGS=$_[0];
  my $realDEG=$_[1];
  my $type=$_[2];
  $LATSIEVE_SIDE=0 if $type eq "gnfs"; # SB
  die("Could not find default parameter file $DEFAULT_PAR_FILE!\n") 
      unless (-e $DEFAULT_PAR_FILE);
  open(IF, $DEFAULT_PAR_FILE);
  my $paramDIGS=0;
  my $paramDEG=0;
  my $howClose=1000;
  while (<IF>) {
    s/#.*//; # Remove comments
    s/\s*//g; # Remove whitespace.
    if (length($_)>0) {
      @_ = split /,/;
      my $t=$_[0];
      if ($t eq $type) {
        my $o=2;
        my $candDIGS=$_[1];
	my $candDEG=$_[$o+0];
# try to properly handle crossover from degree 4 to degree 5
        if ( ($type eq "gnfs" or ! $realDEG or $realDEG == $candDEG or $paramDEG == $candDEG-1)
             ? abs($candDIGS-$realDIGS)<$howClose
             : (abs($candDIGS-$nonPrefDegAdjust-$realDIGS)+$nonPrefDegAdjust)<$howClose )
        {
          $howClose=($type ne "gnfs" and $realDEG and $candDEG != $realDEG)
                    ? abs($candDIGS-$nonPrefDegAdjust-$realDIGS)+$nonPrefDegAdjust
                    : abs($candDIGS-$realDIGS);
          $paramDIGS=$candDIGS;
          $paramDEG=$_[$o+0]; 
          $MAXS1=$_[$o+1]; 
          $MAXSKEW=$_[$o+2]; 
          $GOODSCORE=$_[$o+3]; 
          $EFRAC=$_[$o+4];
          $J0=$_[$o+5]; $J1=$_[$o+6];
          $ESTEPSIZE=$_[$o+7]; 
          $MAXTIME=$_[$o+8];
          $RLIM=$_[$o+9];     $ALIM=$_[$o+10]; 
          $LPBR=$_[$o+11];    $LPBA=$_[$o+12];
          $MFBR=$_[$o+13];    $MFBA=$_[$o+14];
          $RLAMBDA=$_[$o+15]; $ALAMBDA=$_[$o+16]; 
          $QINTSIZE=$_[$o+17];
          $classicalA=$_[$o+18];
          $classicalB=$_[$o+19];
          $QSTEP=$QINTSIZE;
        }
      }
    }
  }
  close(IF);
  $DIGS = ($type eq "gnfs") ? $realDIGS/0.72 : $realDIGS;
                   # 0.72 is inspired by T.Womack's crossover 28/29 for GNFS-144 among other considerations
  if($DIGS>=160) { # the table parameters are easily splined; the table may be not needed at all --SB.
    $RLIM=    $ALIM   = int(0.07*10**($DIGS/60.0)+0.5)*100000;
    $LPBR=    $LPBA   = int(21+$DIGS/25);
    $MFBR=    $MFBA   = ($DIGS<190) ? 2*$LPBR-1 : 2*$LPBR;
    $RLAMBDA= $ALAMBDA= ($DIGS<200) ? 2.5 : 2.6;
    $QINTSIZE=$QSTEP  = 100000;
    $classicalA       = 4000000;
    $classicalB       = 400;
    $paramDIGS        = $realDIGS;
  }
  $DEG = $paramDEG;
  printf "-> Selected default factorization parameters for $paramDIGS digit level.\n";
  if ($type eq "gnfs") {
    if ($realDIGS < 95) { $LATSIEVER=$LATSIEVER_L1; } # just a guess
    elsif ($realDIGS < 110) { $LATSIEVER=$LATSIEVER_L2; } # kept old value here
    elsif ($realDIGS < 140) { $LATSIEVER=$LATSIEVER_L3; } # based on experience in mersenneforum
    elsif ($realDIGS < 158) { $LATSIEVER=$LATSIEVER_L4; } # guess based on experience in mersenneforum
    elsif ($realDIGS < 185) { $LATSIEVER=$LATSIEVER_L5; } # just a wild guess
    else { $LATSIEVER=$LATSIEVER_L6; }
  } else {
    $realDIGS += $nonPrefDegAdjust if ($realDEG and ($paramDEG != $realDEG));
    if ($realDIGS < 120) { $LATSIEVER=$LATSIEVER_L1 } # just a guess
    elsif ($realDIGS < 150) { $LATSIEVER=$LATSIEVER_L2; } # kept old value here
    elsif ($realDIGS < 195) { $LATSIEVER=$LATSIEVER_L3; } # based on experience in mersenneforum
    elsif ($realDIGS < 240) { $LATSIEVER=$LATSIEVER_L4; } # guess based on experience in mersenneforum
    elsif ($realDIGS < 275) { $LATSIEVER=$LATSIEVER_L5; } # guess based on experience in mersenneforum
    else { $LATSIEVER=$LATSIEVER_L6; }
  }
  printf "-> Selected lattice siever: $LATSIEVER\n";
}


######################################################
sub loadPolselParamsPol5 {
######################################################
# 
# These are default parameters for polynomial selection using the
# Kleinjung/Franke tool. 
# arg0 = number of digits in N.

  die("loadPolselParamsPol5(): Insufficient arguments!\n") 
      unless ($#_ >=0);
  my $realDIGS=$_[0];
  my $paramDIGS=0;
  if($NO_DEF_NM_PARAM || $d<100) {
    $d=$realDIGS;
    $search_a5step=1;
    $npr      = int($d/13-4.5); $npr=4 if $npr<4;
    $normmax  = sprintf("%.3G", 10**(0.163  * $d - 1.4794));
    $normmax1 = sprintf("%.3G", 10**(0.1522 * $d - 1.6969));
    $normmax2 = sprintf("%.3G", 10**(0.142  * $d - 2.6429));
    $murphymax= sprintf("%.3G", 10**(-0.0569* $d - 2.8452));
    $maxPSTime= int(0.000004/$murphymax);
    printf "-> Selected polsel parameters for $d digit level.\n";
    return;
  }
  die("Could not find default parameter file $DEFAULT_POLSEL_PAR_FILE!\n") 
      unless (-e $DEFAULT_POLSEL_PAR_FILE);
  open(IF, $DEFAULT_POLSEL_PAR_FILE);
  my $howClose=1000;
  while (<IF>) {
    s/#.*//; # Remove comments
    s/\s*//g; # Remove whitespace.
    if (length($_)>0) {
      @_ = split /,/;
      my $d=$_[0];
      if (abs($d-$realDIGS)<$howClose) {
        $o=1;
        $howClose=abs($d-$realDIGS);
        $paramDIGS=$d;
        $maxPSTime=60*$_[$o+0];
        $search_a5step=$_[$o+1]; 
        $npr=$_[$o+2]; 
        $normmax=$_[$o+3]; 
        $normmax1=$_[$o+4];
        $normmax2=$_[$o+5];
        $murphymax=$_[$o+6];
      }
    }
  }
  close(IF);
  printf "-> Selected default polsel parameters for $paramDIGS digit level.\n";
}

#######################################################
sub terminate_search {
    $terminate_job=1;
    printf "Terminated on $pname by SIGTERM\n";
}

#######################################################
sub runPol5 {
  $projectname=$NAME.".polsel";
  use Sys::Hostname;
  $host=hostname;
  $pname="$projectname.$host.$$";

  open(OF, ">$pname.data");
  printf OF "N ".$N;
  close(OF);
  my $terminate_job=0;
  local $SIG{'TERM'}='terminate_search';

  loadPolselParamsPol5(length($N));
  $maxPSTime *= $polySelTimeMultiplier;
  my $hmult=1e3;
  loadDefaultParams(length($N), 5, "gnfs");
  my %bestpolyinf = ();
  $bestpolyinf{Murphy_E} = 0;

  my $H=0;
  my $startTime = time;
  for(my $nerr=0;$terminate_job==0 && $nerr<2;) {
    my $HH=$H+$search_a5step;
    printf "-> Searching leading coefficients from %d to %d.\n", $H*$hmult+1, $HH*$hmult;
    $cmd="$NICE \"$POL51M0\" -b $pname -v -v -p $npr -n $normmax -a $H -A $HH > $pname.log";
    printf("=> $cmd\n");
    my $res=system($cmd);
   if (!$res) { # lambda-comp related errors can be skipped and some polys are then found
                # ull5-comp related are probably fatal, but let the elapsed time take care of them
    $nerr=0;
    open(GR,"$pname.log");
    my @logout=<GR>;
    close(GR);
    $suc = grep(/success/, @logout);
    if($suc!=0) {
      $cmd="$NICE \"$POL51OPT\" -b $pname -v -v -n $normmax1 -N $normmax2 -e $murphymax > $pname.log";
      printf("=> $cmd\n");
      $res=system($cmd);
      die "Abnormal return value $res. Terminating...\n" if ($res);
      concat("$pname.51.m", ">>", "$projectname.51.m.all");
      open(GR,"<$pname.cand");
      my %polyinf = ();
      my $changed = 0;
      while(<GR>) {
        chomp; s/\r?$//;
        if (s/^BEGIN POLY//) {
          s/^ #//;
          %polyinf = split;
        } elsif (/^END POLY/) {
          if ($polyinf{Murphy_E} > $bestpolyinf{Murphy_E}) {
            %bestpolyinf = %polyinf;
            $changed = 1;
          }
          %polyinf = ();
        } else {
          my ($key, $val) = split;
          $key =~ s/^X/c/;
          $polyinf{$key} = $val;
        }
      }
      close(GR);
      if ($changed) {
        foreach my $key (sort keys %bestpolyinf) {
          print "$key: $bestpolyinf{$key}\n";
        }
        open(BP, ">$NAME.poly");
        print BP "name: $NAME\n";
        print BP "n: $N\n";
        foreach my $key (reverse sort keys %bestpolyinf) {
          if ($key =~ /c\d+/ or $key =~ /Y\d+/) {
            print BP "$key: $bestpolyinf{$key}\n";
          } elsif ($key =~ /skewness/) {
            print BP "skew: $bestpolyinf{$key}\n";
          } else {
            print BP "# $key $bestpolyinf{$key}\n";
          }
        } 
        print BP "type: gnfs\n";
        print BP "rlim: $RLIM\n";
        print BP "alim: $ALIM\n";
        print BP "lpbr: $LPBR\n";
        print BP "lpba: $LPBA\n";
        print BP "mfbr: $MFBR\n";
        print BP "mfba: $MFBA\n";
        print BP "rlambda: $RLAMBDA\n";
        print BP "alambda: $ALAMBDA\n";
        print BP "qintsize: $QINTSIZE\n";
        close(BP);
      }
      concat("$pname.cand", ">>", "$projectname.cand.all");
      unlink "$pname.cand";
    }
    printf "-> =====================================================\n";
    printf("-> Best score so far: %e (goodScore=%e)\n",$bestpolyinf{Murphy_E},$murphymax);
    printf "-> =====================================================\n\n";
    unlink "$pname.log";
    unlink "$pname.51.m";
   }
    my $nowTime = time;
    if ($nowTime - $startTime > $maxPSTime) {
      $terminate_job=1;
    }
    $H=$HH;
  }
  unlink "$pname.data";

  # What remains to be done is to:
  # (1) Search the x.cand.x file for the best candidate.
  # (2) load default factorization parameters.
  # (3) Output a GGNFS polynomial file with the poly and parameters.
  # (4) Delete the intermediate files.
  # In fact, we should probably do (1) above and some file renaming,
  # so that we don't have to search through the whole (growing) file
  # at each iteration. Keep the best poly in a seperate file and
  # maybe even just delete the other candidates.
  #
  # S.Chong: 
  # (1) through (3) are done, (4) if you don't care about leaving the *.all
  # files lying around.  Only the best candidate is saved in a .poly file,
  # to look at the rest you'll have to dig through the *.cand.all file.
  #     The next thing to do is figure out how to support multiple polysel
  # clients.  The filenames shouldn't need to be changed, but we'll need to
  # get/update $H from a lock-protected file and also lock-protect the *.all
  # files.  Then the rest should be easy.  I imagine multi-client sieving
  # could be enhanced similarly around $Q0.
}

######################################################
sub runPolyselect {
######################################################
# We will start with a higher leading coefficient divisor. When it
# appears that we are searching in an interesting range, it will
# be backed down so that the resulting range can be searched with
# a finer resolution. This means that from time to time, the same
# poly will be found several times as we hone in on a region.
  my @lcdChoices=(2,4,4,12,12,24,24,48,48,144,144,720,5040);
  my $lcdLevel=4+(length($N)-70)/10;
  if ($lcdLevel < 0) { $lcdLevel=0;}
  if ($lcdLevel > 12) { $lcdLevel=12;}
  my $E0=1;
  my $firstGoodTime=0;
  printf "-> Starting search with leading coefficient divisor $lcdChoices[$lcdLevel].\n";

  loadDefaultParams(length($N), 0, "gnfs");
  $MAXTIME *= $polySelTimeMultiplier;
  my $E1=$E0+$ESTEPSIZE;
  my $goodPFound=0;
  my $bestScore=0.0;
  my $startTime = time;
  my $done=0;
  my $bestLC=1;
  my $multiplier=0.75;
  while (!$done) {
    $LCD=$lcdChoices[$lcdLevel];
    open(OF, ">$NAME.polsel");
    printf OF "name: $NAME\n";
    printf OF "n: $N\n";
    printf OF "deg: $DEG\n";
    printf OF "bf: best.poly\n";
    printf OF "maxs1: $MAXS1\n";
    printf OF "maxskew: $MAXSKEW\n";
    printf OF "enum: $LCD\n";
    printf OF "e0: $E0\n";
    printf OF "e1: $E1\n";
    $CUTOFF=0.75*$GOODSCORE;
    printf OF "cutoff: $CUTOFF\n";
    printf OF "examinefrac: $EFRAC\n";
    printf OF "j0: $J0\n";
    printf OF "j1: $J1\n";
    close(OF);
    my $cmd="$NICE \"$POLYSELECT\" -if $NAME.polsel";
    print "=>$cmd\n" if($ECHO_CMDLINE);
    $res=system($cmd);
    die "Return value $res. Terminating...\n" if ($res);
    # Find the score of the best polynomial:
    # E(F1,F2) = 
    open(INFO, "best.poly");
    my @polyinf=<INFO>;
    close(INFO);
    my @TMP=grep(/E\(F1,F2\) =/, @polyinf);
    my $SCORE=$TMP[0];
    $SCORE =~ s/.*E\(F1,F2\) =//;
    @TMP=grep(/^c$DEG/, @polyinf);
    $_=$TMP[0];
    /^c$DEG:\s(\d*)/;
    $LC = $1;
    if (($SCORE > $multiplier*$GOODSCORE)&&($LC != $bestLC) && ($LC != $lastLC)) {
      $multipler *= 1.1;
      if ($multiplier > 0.9) { $multiplier = 0.9; }
      $lastLC = $LC;
      my $newLCDLevel = $lcdLevel-1;
      if ($newLCDLevel < 0) { $newLCDLevel=0; }
      $E0 = $E0*$lcdChoices[$lcdLevel]/$lcdChoices[$newLCDLevel] - $ESTEPSIZE;
      if ($lcdLevel != $newLCDLevel) {
        printf "-> Leading coefficient divisor dropped from $lcdChoices[$lcdLevel] to $lcdChoices[$newLCDLevel].\n";
      }
      $lcdLevel = $newLCDLevel;
    }
    if (($SCORE > $bestScore)&&($LC != $bestLC)) { 
      $bestScore = $SCORE;
      $bestLC=$LC;
      $lastBestTime = time;
      rename "best.poly", "thebest.poly";
      # We should now fill in the missing parameters with the defaults
      # loaded in from table. Do this now, so that the user has the
      # option to kill the script and still have a viable poly file.
      open(IF, "thebest.poly");
      open(OF, ">$NAME.poly");
      while (<IF>) {
        chomp;
        if (/rlim:/) { $_="rlim: $RLIM"; }
        if (/alim:/) { $_="alim: $ALIM"; }
        if (/lpbr:/) { $_="lpbr: $LPBR"; }
        if (/lpba:/) { $_="lpba: $LPBA"; }
        if (/mfbr:/) { $_="mfbr: $MFBR"; }
        if (/mfba:/) { $_="mfba: $MFBA"; }
        if (/rlambda:/) { $_="rlambda: $RLAMBDA"; }
        if (/alambda:/) { $_="alambda: $ALAMBDA"; }
        if (/qintsize:/) { $_="qintsize: $QINTSIZE"; }
        printf OF "$_\n";
      }
      printf OF "type: gnfs\n";
      close(OF);
      close(IF);
      unlink "thebest.poly";
      unlink "best.poly";
    }
    printf "-> =====================================================\n";
    printf("-> Best score so far: %f (goodScore=%f)     \n",$bestScore,$GOODSCORE);
    printf "-> =====================================================\n";

    $E0 += $ESTEPSIZE;
    $E1 = $E0 + $ESTEPSIZE;
    $done=0;
    if ($bestScore > 1.4*$GOODSCORE) { $goodPFound=1; }
    if ($goodPFound) {
      # We will allow another 5 minutes just in case there happens to
      # be a really good poly nearby (or the 'goodScore' value was too low)
      my $elapsed = time - $lastBestTime;
      if ($elapsed > 300) { 
        $done=1;
      }
    }
    my $nowTime=time;
    if ($nowTime - $startTime > $MAXTIME) { $done=1; }
  }
  printf("-> Using poly with score=%f\n", $bestScore);

  unlink "$NAME.polsel";
}

######################################################
sub changeParams {
######################################################
# Change the factor base sizes for a factorization
# which has already been started. This is done by
# dumping the raw siever output from the rels.bin files,
# deleting the rels.bin files, re-setting the parameters,
# and reprocessing all of the relations.
  open(LF, ">>$LOGFILE");
  print LF "Parameters change detected.\n";
  close(LF);

  print "-> Parameter change detected...\n";
  print "-> Dumping relations...\n";
  $cmd="$NICE \"$PROCRELS\" -fb $NAME.fb -prel $RELSBIN -dump";
  print "=>$cmd\n" if($ECHO_CMDLINE);
  $res=system($cmd);
  die "Return value $res. Terminating...\n" if ($res);
  unlink <$RELSBIN*>, <cols*>, <deps*>, 'factor.easy', <lpindex*>;
  unlink <$NAME.*.afb.*>;

  print "-> Making new factor base files...\n";
  $cmd="$NICE \"$MAKEFB\" -rl $RLIM -al $ALIM -lpbr $LPBR -lpba $LPBA $LARGEPRIMES -of $NAME.fb -if $NAME.poly";
  print "=>$cmd\n" if($ECHO_CMDLINE);
  $res=system($cmd);
  die "Return value $res. Terminating...\n" if ($res);

  print "-> Reprocessing siever output...\n";
  $i=0;
  while (-e "spairs.dump.$i") {
    $cmd="$NICE \"$PROCRELS\" -fb $NAME.fb -prel $RELSBIN -newrel spairs.dump.$i -nolpcount";
    print "=>$cmd\n" if($ECHO_CMDLINE);
    $res=system($cmd);
    $i++;
  }
  # Update the paramfile, used by this script to detect changes
  # in parameter settings.
  open(INFO, ">$PARAMFILE");
  print INFO "rlim: $RLIM\n"; 
  print INFO "alim: $ALIM\n"; 
  print INFO "lpbr: $LPBR\n"; 
  print INFO "lpba: $LPBA\n"; 
  close(INFO);
} 
  
###########################################################
sub checkParamFile {
###########################################################
# Check two things:
# (1) Does a $PARAMFILE exist? If not, create it.
# (2) Are the values in $PARAMFILE the same as the current
#     values? If not, call changeParams to sync everything up.
  if (!(-e $PARAMFILE)) {
    print "-> Creating param file to detect parameter changes...\n";
    open(INFO, ">$PARAMFILE");
    print INFO "rlim: $RLIM\n"; 
    print INFO "alim: $ALIM\n"; 
    print INFO "lpbr: $LPBR\n"; 
    print INFO "lpba: $LPBA\n"; 
    close(INFO);
    return ;
  }
  # Okay - it exists. We should check it to see if the
  # parameters are the same as the current ones.
  open(INFO, $PARAMFILE);
  my @lastParams=<INFO>;
  close(INFO);
  my @TMP=grep(/rlim:/, @lastParams);
  my $OLDRLIM=$TMP[0];
  $OLDRLIM =~ s/.*rlim://;
  @TMP=grep(/alim:/, @lastParams);
  my $OLDALIM=$TMP[0];
  $OLDALIM =~ s/.*alim://;
  @TMP=grep(/lpbr:/, @lastParams);
  my $OLDLPBR=$TMP[0];
  $OLDLPBR =~ s/.*lpbr://;
  @TMP=grep(/lpba:/, @lastParams);
  my $OLDLPBA=$TMP[0];
  $OLDLPBA =~ s/.*lpba://;
  if ($OLDRLIM != $RLIM || $OLDALIM != $ALIM ||
      $OLDLPBR != $LPBR || $OLDLPBA != $LPBA) {
    changeParams;
  } else {
    print "-> No parameter change detected. Resuming.\n";
  }
}

###########################################################
sub plotLP {
###########################################################
  if ($CHECK_BINARIES) {
    return unless (-x $PLOT);
  }
  open(OUTF, ">.lprels") || return;
  print OUTF "0, 0\n";
  open(OUTRELS, ">.rels") || return;
  print OUTRELS "0, 0\n";
  open(LOG, $LOGFILE) || return;
  while ($_ = <LOG>) {
    chomp;
    tr/a-z/A-Z/; # Convert to upper case.
    if ( /LARGEPRIMES/) {
      s/\[.*]\s*//;  # Strip out the date.
      s/(LARGEPRIMES: |RELATIONS: )//g;  # Strip out the labels.
      s/\s//g; # Remove whitespace.
      @_ = split /,/;
      $excessLP=$_[0]-$_[1];
      print OUTF "$_[1], $excessLP\n";
    } elsif (/FINALFF/) {
      s/\[.*]\s*//;  # Strip out the date.
      s/(RELS:|INITIALFF:\d*,|FINALFF:)//g;  # Strip out the labels.
      s/\s//g; # Remove whitespace.
      @_ = split /,/;
      print OUTRELS "$_[0], $_[1]\n";
    }
  }
  close(OUTF);
  close(OUTRELS);
  close(LOG);
  $ENV{'XAXIS'}='Total relations';
  $ENV{'YAXIS'}="";
  $cmd="\"$PLOT\""." xprimes.png 'ExcessLargePrimes' .lprels";
  print "=>$cmd\n" if($ECHO_CMDLINE);
  $res=system($cmd);
  die "Return value $res. Terminating...\n" if ($res);
  $ENV{'XAXIS'}='Total relations';
  $ENV{'YAXIS'}="Full relation-sets";
  $cmd="\"$PLOT\""." relations.png 'TotalFF' .rels";
  print "=>$cmd\n" if($ECHO_CMDLINE);
  $res=system($cmd);
  die "Return value $res. Terminating...\n" if ($res);
  unlink '.lprels';
  unlink '.rels';
}

######################################################
sub checkParams {
######################################################
  if ($CHECK_BINARIES) {
    $missing .= 'msieve ' unless (-x $MSIEVE);
    $missing .= '(lattice siever) ' unless (-x $LATSIEVER);
    unlink '.lprels';
    unlink '.rels';
    open(OF, ">.rels");
    print OF "N\n";
    close(OF);
    $CAT = "" if system("\"$CAT\" .rels > .lprels"); # we will use perl then
    unlink '.lprels';
    unlink '.rels';
  }
  if ($missing) {
    print "-> Could not find GGNFS programs: $missing.\n";
    print "-> Did you set GGNFS_BIN_PATH properly in this script?\n";
    print "-> It is currently set to:";
    print "-> GGNFS_BIN_PATH=$GGNFS_BIN_PATH\n";
    exit -1;
  }
  die("Error: 'n' not supplied!\n") unless ($N);
  die("Error: 'm' not supplied!\n") unless ($M || defined $COEFHASH{Y1});
  die("Error: polynomial not supplied!\n") unless (defined $COEFHASH{'c'.$DEGREE});
  unless ($SKEW) {
    $SKEW = abs($COEFHASH{'c0'}/$COEFHASH{'c'.$DEGREE})**(1.0/$DEGREE);
    print "-> Using calculated skew $SKEW\n";
  }
  die("Error: 'rlim' not supplied!\n") unless ($RLIM);
  die("Error: 'alim' not supplied!\n") unless ($ALIM);
  die("Error: 'lpbr' not supplied!\n") unless ($LPBR);
  die("Error: 'lpba' not supplied!\n") unless ($LPBA);
  die("Error: 'mfbr' not supplied!\n") unless ($MFBR);
  die("Error: 'mfba' not supplied!\n") unless ($MFBA);
  die("Error: 'rlambda' not supplied!\n") unless ($RLAMBDA);
  die("Error: 'alambda' not supplied!\n") unless ($ALAMBDA);
  die("Error: 'qintsize' not supplied!\n") unless ($QSTEP);
}

######################################################
sub makeJobFile {
######################################################
# arg0 = filename
# arg1 = first q value
# arg2 = q range size
# arg3 = client num
# arg4 = number of clients
# arg5 = A0 value for classical sieving
# arg6 = A1 value for classical sieving
# arg7 = B0 value for classical sieving
# arg8 = B1 value for classical sieving
# Note: arguments 5-8 are only used if arg1=arg2=0. Otherwise,
# they need not even be supplied.
  if ($#_ < 4) { die("makeJobFile() : Not enough arguments!\n") };
  $FNAME=$_[0];
  my $firstQ = $_[1];
  my $qrSize = $_[2];
  my $client = $_[3]-1; # Convert to 0,1,..., numClients-1.
  my $numClients = $_[4];
  my $A0=0; my $A1=0;
  my $B0=0; my $B1=0;
  if ($#_ >= 8) {
    $A0 = $_[5]; $A1 = $_[6];
    $B0 = $_[7]; $B1 = $_[8];
  }
  my $sieveType=0;  

  if (($firstQ >0) || ($qrSize > 0)) {
    $sieveType=1;
    # First, find the proper q0 and qrSize for this client.
    # The idea is that client 'c' should sieve over ranges
    #     [QSTART + k*qrSize, QSTART + (k+1)qrSize]
    # with k == c (mod numClients). Thus, we need to find the
    # first such range containing q0.
    my $k=int(($firstQ - $QSTART)/$qrSize);
    if ($k*$qrSize + $QSTART > $firstQ) {
      # Could this happen from rounding error? 
      $k--;
    }
    while (($k % $numClients) != $client) {
      $k += 1;
    }
    $q0 = $QSTART + $k*$qrSize;
    $q1 = $q0 + $qrSize;
    printf "-> makeJobFile(): q0=$q0, q1=$q1.\n";
    if ($firstQ >= $q1) {
      $k += $numClients;
      $q0 = $QSTART + $k*$qrSize;
      $q1 = $q0 + $qrSize;
    } else {
      if ($firstQ > $q0) {
        $q0 = $firstQ;
      }
      $qrSize = $q1 -$q0;
    }
    printf "-> makeJobFile(): Adjusted to q0=$q0, q1=$q1.\n";
    open(OF, ">>$LOGFILE");
    print OF "-> makeJobFile(): Adjusted to q0=$q0, q1=$q1.\n";
    close(OF);
    $Q0 = $q0;
    $Q1 = $q1;
    $thisQRSize = $qrSize;
  }
  
  if ($NUM_THREADS==1) {
    unlink $FNAME;
    open(OUTF, ">$FNAME");
    print OUTF "n: $N\nm: $M\n";
    # The polynomial coefficients:
    for ($i=0; length(${COEF[$i]}) > 0; $i += 2) {
      print OUTF "${COEF[$i]} ${COEF[${i}+1]}\n";
    }
    print OUTF "skew: $SKEW\n";
    if ($sieveType==1) {
      if ($LATSIEVE_SIDE) {
        if ($RLIM > $q0) {
          $sieverRL = $q0-1;
          unlink <$JOBNAME.afb.1>;
        }
        else { $sieverRL = $RLIM; }
        $sieverAL = $ALIM;
      } else { 
        if ($ALIM > $q0) {
          $sieverAL = $q0-1;
          unlink <$JOBNAME.afb.0>;
        }
        else { $sieverAL = $ALIM; }
        $sieverRL = $RLIM;
      }
      print OUTF "rlim: $sieverRL\n";
      print OUTF "alim: $sieverAL\n";
    }
    else {
      print OUTF "rlim: $RLIM\n";
      print OUTF "alim: $ALIM\n";
    }
    print OUTF "lpbr: $LPBR\n";
    print OUTF "lpba: $LPBA\n";
    print OUTF "mfbr: $MFBR\n";
    print OUTF "mfba: $MFBA\n";
    print OUTF "rlambda: $RLAMBDA\n";
    print OUTF "alambda: $ALAMBDA\n";
    if ($sieveType==1) {
      print OUTF "q0: $q0\n";
      print OUTF "qintsize: $qrSize\n"; 
      print OUTF "#q1:$q1\n";
    } else {
      print OUTF "a0: $A0\n";
      print OUTF "a1: $A1\n";
      print OUTF "b0: $B0\n";
      print OUTF "b1: $B1\n";
    }
    close(OUTF); 
  }
  else {
    for ($j=1; $j<=$NUM_THREADS; $j++) {
      my $FNAMET = $FNAME.".T".$j;
      my $qrSizeT = int($qrSize/$NUM_THREADS);
      my $q0T = $q0 + ($j-1)*$qrSizeT;
      my $q1T = $q0T + $qrSizeT;
      unlink $FNAMET;
      open(OUTF, ">$FNAMET");
      print OUTF "n: $N\nm: $M\n";
      # The polynomial coefficients:
      for ($i=0; length(${COEF[$i]}) > 0; $i += 2) {
        print OUTF "${COEF[$i]} ${COEF[${i}+1]}\n";
        }
      print OUTF "skew: $SKEW\n";
      if ($sieveType==1) {
        if ($LATSIEVE_SIDE) {
          if ($RLIM > $q0) {
            $sieverRL = $q0-1;
            unlink <$FNAMET.afb.1>;
          }
          else { $sieverRL = $RLIM; }
          $sieverAL = $ALIM;
        } else { 
          if ($ALIM > $q0) {
            $sieverAL = $q0-1;
            unlink <$FNAMET.afb.0>;
          }
          else { $sieverAL = $ALIM; }
          $sieverRL = $RLIM;
        }
        print OUTF "rlim: $sieverRL\n";
        print OUTF "alim: $sieverAL\n";
      }
      else {
        print OUTF "rlim: $RLIM\n";
        print OUTF "alim: $ALIM\n";
      }
      print OUTF "lpbr: $LPBR\n";
      print OUTF "lpba: $LPBA\n";
      print OUTF "mfbr: $MFBR\n";
      print OUTF "mfba: $MFBA\n";
      print OUTF "rlambda: $RLAMBDA\n";
      print OUTF "alambda: $ALAMBDA\n";
      if ($sieveType==1) {
        print OUTF "q0: $q0T\n";
        print OUTF "qintsize: $qrSizeT\n"; 
        print OUTF "#q1:$q1T\n";
      } else {
        print OUTF "a0: $A0\n";
        print OUTF "a1: $A1\n";
        print OUTF "b0: $B0\n";
        print OUTF "b1: $B1\n";
      }
      close(OUTF); 
    }
  }
}

###########################################################
sub get_parm_int($$)
###########################################################
{
  my $data = shift;
  my $parm = shift;

  my @PARMLINES=grep(/^$parm:/, @$data);
  if (@PARMLINES and $PARMLINES[0] =~ /^$parm:\s*(-?\d+)/) { return $1; }
  return undef;
}

######################################################
sub readParams {
######################################################

  # Read default parameters first. Then we will just override
  # by any user-supplied parameters.
  open(PF, $NAME.".poly");
  my @thisData=<PF>;
  close(PF);

  $N = get_parm_int(\@thisData, 'n');

  # Find the polynomial degree.
  %COEFVALS = ();
  my @COEFLINE=grep(/^c\d+:/, @thisData);
  $D=0;
  while($_ = shift @COEFLINE) {
    # Grab the coefficient index
    # First char of line 'c' followed by a digit string.
    my ($key, $val) = /^(c\d+):\s*(-?\d+)/;
    $key =~ s/c//;
    if ($key > $D) { $D=$key; }
    print "-> Warning: redefining c$key\n" if (defined $COEFVALS{$key});
    $COEFVALS{$key} = $val;
  }
  $DEGREE = get_parm_int(\@thisData, 'deg');
  $DEGREE = $D if (!defined $DEGREE);
  if ($DEGREE != $D) {
    print "-> Error: poly file specifies degree $DEGREE but highest poly\n";
    print "->   coefficient given is c$D!\n";
    exit;
  }
  my $commonfac = new Math::BigInt '0';
  foreach my $key (reverse sort keys %COEFVALS) {
    #print "-> c$key: $COEFVALS{$key}\n";
    $commonfac = $commonfac->bgcd($COEFVALS{$key});
  }
  unless (!$CHECK_POLY or $commonfac->is_one()) {
     print "-> Error: poly coefficients have a common factor $commonfac. Please divide it out.\n";
     exit;
  }
  my ($numer, $denom);
  $denom = get_parm_int(\@thisData, 'Y1');
  $numer = get_parm_int(\@thisData, 'Y0');
  $M = get_parm_int(\@thisData, 'm');
  if ($denom && $numer) {
    $numer = new Math::BigInt $numer;
    $denom = new Math::BigInt $denom;
    if ($denom->is_neg()) { $denom = -$denom; }
    else { $numer = -$numer; }
#    print "-> Common root is $numer / $denom\n";
    # paranoia if CHECK_POLY is set
    unless (!$CHECK_POLY or (my $Ygcd = Math::BigInt::bgcd ($numer, $denom))->is_one()) {
      print "-> Error: Y1 and Y0 have a common factor $Ygcd. Please divide it out.\n";
      exit;
    }
    if ($M) {
      my $Yval = $denom->copy();
      $Yval->bmul($M);
      $Yval->bsub($numer);
      $Yval->bmod($N);
      unless (!$CHECK_POLY or $Yval->is_zero()) {
        print "-> Error: Y1*m + Y0 != 0 mod n!\n";
        exit;
      }
    } else { undef $M; }
  }
  my $polyval = new Math::BigInt '0';
  if ($denom && $numer) {
    my $subtotal = new Math::BigInt;
    for my $i (0..$DEGREE) {
      $subtotal = $COEFVALS{$i} * $numer**$i * $denom**($DEGREE - $i);
      $polyval->badd($subtotal);
    }
  } else {
#    print "-> Common root is $M\n";
    for my $i (reverse 0..$DEGREE) {
      $polyval->bmul($M);
      $polyval->badd($COEFVALS{$i});
    }
  }
  unless (!$CHECK_POLY or $polyval > 0) {
    print "-> Warning: evaluated polynomial value $polyval is negative or zero.\n";
    print "->   This is at least a little strange.\n";
  }
  my $remainder = $polyval->copy();
  $remainder->bmod($N);
  unless (!$CHECK_POLY or $remainder->is_zero()) {
    print "-> Error: evaluated polynomial value $polyval is not a multiple of n!\n";
    exit;
  }
#  print "-> Evaluated value is $polyval\n";

  my @TYPELINE=grep(/type:/, @thisData);
  $TYPE=$TYPELINE[0];
  $TYPE =~ s/.*type: //;
  chomp $TYPE; $TYPE =~ s/\r+$//;
  if ($TYPE =~ /snfs/) {
    # We need the difficulty level of the number, which may be
    # noticably larger than the number of digits.
    $SNFS_DIFFICULTY = (new Math::BigFloat $polyval)->babs->blog(10,6);

    printf "-> SNFS_DIFFICULTY is about $SNFS_DIFFICULTY.\n";
    loadDefaultParams($SNFS_DIFFICULTY->bstr(), $DEGREE, $TYPE);
  } elsif ($TYPE =~ /gnfs/) {
    my $logN = (new Math::BigFloat $N)->babs->blog(10);
#    print "-> Log N is about $logN.\n";

    loadDefaultParams(length($N), $DEGREE, $TYPE);
  } else {
    printf "-> Error: poly file should contain one of the following lines:\n";
    printf "-> type: snfs\n";
    printf "-> type: gnfs\n";
    printf "-> Please add the appropriate line and re-run.\n";
    exit;
  }

  # Now look for user-supplied parameters.
  $Q0=0;
  open(PF, $NAME.".poly");
  while (<PF>) {
    chomp;
    s/#.*//; # Remove comments
    s/\s*//g; # Remove whitespace.
    @_ = split /:/;
    my $token=$_[0]; my $val=$_[1];
    if ((length($token)>0) && (length($val)>0)) {
      if ($token eq "n") { $N=$val; }
      elsif ($token eq "m") { $M=$val; }
      elsif ($token eq "rlim") { $RLIM=$val; }
      elsif ($token eq "alim") { $ALIM=$val; }
      elsif ($token eq "lpbr") { $LPBR=$val; }
      elsif ($token eq "lpba") { $LPBA=$val; }
      elsif ($token eq "mfbr") { $MFBR=$val; }
      elsif ($token eq "mfba") { $MFBA=$val; }
      elsif ($token eq "rlambda") { $RLAMBDA=$val; }
      elsif ($token eq "alambda") { $ALAMBDA=$val; }
      elsif ($token eq "knowndiv") { $KNOWNDIV=$val; }
      elsif ($token eq "skew") { $SKEW=$val; }
      elsif ($token eq "q0") { $Q0=$val; }
      elsif ($token eq "qintsize") { $QSTEP=$val; }
      elsif ($token eq "lss") { $LATSIEVE_SIDE=$val; }
      elsif ($token eq "mrif") { $maxRelsInFF=$val; }
      elsif (($token =~ /c./) || ($token =~ /Y./)) {
        push(@COEF, $token.":");
        push(@COEF, $val);
        $COEFHASH{$token} = $val;
      }
    }
  }
  if ($KNOWNDIV)
  {
    my $DIVISOR = new Math::BigInt($KNOWNDIV);
    die ("-> Error: knowndiv $DIVISOR does not divide N!\n") if ($N % $DIVISOR);
    $NDIVFREE = $N / $DIVISOR;
    $N = $NDIVFREE;
    $KNOWNDIVOPT = "-knowndiv ".$KNOWNDIV;
  }
  else { $NDIVFREE = $N; }
  die ("-> Error:  N is probably prime!\n") if (probab_prime_p($NDIVFREE, 10));
  if ($Q0==0) {
    $Q0 = ($LATSIEVE_SIDE) ? $RLIM/2 : $ALIM/2;
  }
  $QSTART=$Q0;
  checkParamFile;
}

######################################################
sub setup {
######################################################

  # Should we resume from an earlier run? #
  $resume=-1;
  concat("$JOBNAME.T1", ">", "$JOBNAME") if (-e "$JOBNAME.T1");
  if (-e $JOBNAME) {
    if ($PROMPTS) {
      print "-> It appears that an earlier attempt was interrupted in progress. Resume? (y/n) ";
      do {
        $_=getc;
        if (($_ eq "Y") || ($_ eq "y")) { $resume=1; }
        elsif (($_ eq "N") || ($_ eq "n")) { $resume=0; }
      } while ($resume < 0);
      printf "\n";
    } else { $resume=1; }
  }
      
  # The below is approx. 0.2*(pi(LPBA) + pi(LPBR)).  
  # It's so small because, for small LPBA, it really overestimates.
  $MINRELS=int(0.2*1.442695*( (2**$LPBA)/$LPBA + (2**$LPBR)/$LPBR));

  ############################################
  # Setup the parameters for sieving ranges. #
  ############################################
  if ($resume != 1) {
    if ($CLIENT_ID == 1) {
      # Clean up any junk leftover from an earlier attempt.
      unlink $LOGFILE, <cols*>, <deps*>, 'factor.easy', <lpindex*>;
      unlink <rels*>, 'spairs.out', 'spairs.save.gz', $NAME.'.fb', <*.afb.0>, <*.afb.1>;
      unlink '.params';

      # Was a discriminant divisor supplied?
      if ($DD) {
        open(INFO, $LOGFILE);
        print INFO "$DD\n";
        close(INFO);
      }
      
      # Create msieve files
      open(OF, ">$ININAME");
      print OF "$N\n";
      close(OF);

      open(OF, ">$DATNAME");
      print OF "N $N\n";
      close(OF);

      open(OF, ">$FBNAME");
      print OF "N $N\n";
      print OF "SKEW $SKEW\n";
      if(!$COEFHASH{Y1}) {
        $COEFHASH{Y1}=1;
        $COEFHASH{Y0}="-".$M;
      }
      for my $i (reverse 0..$DEGREE) {
          print OF "A$i $COEFVALS{$i}\n";
      }
      print OF "R1 $COEFHASH{Y1}\n";
      print OF "R0 $COEFHASH{Y0}\n";
      print OF "FAMAX $ALIM\n";
      print OF "FRMAX $RLIM\n";
      printf(OF "SALPMAX %d\n",2**$LPBA);
      printf(OF "SRLPMAX %d\n",2**$LPBR);
      close(OF);
    }
    $Q0=$QSTART;
  } else {
    # Get the Q0 value from tmp.job and just restart from there.
    if (-e $JOBNAME) {
      open(INFO, $JOBNAME);
      @lastJobF=<INFO>;
      close(INFO);
      @Q0LINE=grep(/q0:/, @lastJobF);
      $Q0=$Q0LINE[0];
      $Q0 =~ s/.*q0://;
      chomp $Q0;
      $Q0 =~ s/\s//g;
      if (!($Q0)) {
        printf "=> Could not recover q0: field from job file $JOBNAME!\n";
        $Q0=$QSTART;
      }
    } else {
      print "-> File $JOBNAME does not exist. Could not determine a starting q value!\n";
      print "-> Please enter a starting point for the special q: ";
      open(INFO, '-');
      $Q0=<INFO>;
      close(INFO);
    }
  }
}

###############################################################
sub classicalSieve {
# arg0 = A value, to sieve [-A,A]
# arg1 = B0
# arg2 = B1, to sieve for b values in [B0, B1].

  if (!($DOCLASSICAL)) { return };

  if ($#_ < 2) { die("classicalSieve() : Not enough arguments!\n") };
  my $A   = $_[0];
  my $B0  = $_[1];
  my $B1  = $_[2];
  my $maxB = 0;
  my $lastline = 0;

  # First, scan the line file and find the largest b-value that was sieved.
  my $LINEFILE = $DATNAME.".line"; 
  if (-e $LINEFILE)  {
    open(LOG, "$LINEFILE");
    my $junk = <LOG>;
    $lastline = <LOG>;
    chomp $lastline;
    close(LOG);
    if ($lastline > $maxB) { $maxB = $lastline;}
  }
  if ($maxB < $B0) { $maxB = $B0; }
  if ($maxB >= $B1) { return ;} 

  open(OF, ">>$FBNAME");
  print OF "SLINE $A\n";
  close(OF);

  printf "-> Line file scanned: resuming classical sieve from b=$maxB.\n";
  $cmd="$NICE \"$MSIEVE\" -s $DATNAME -l $LOGFILE -i $ININAME -v -nf $FBNAME -t $NUM_CPUS -ns $maxB,$B1";
  print "=>$cmd\n" if($ECHO_CMDLINE);
  $res=system($cmd);
  die("Interrupted. Terminating...\n") if ($res);
}

############################################
######## Begin execution here ##############
############################################

$SIG{'INT'}=\&sigDie;
print 
"-> ___________________________________________________________
-> |        This is the factMsieve.pl script for GGNFS.       |
-> | This program is copyright 2004, Chris Monico, and subject|
-> | to the terms of the GNU General Public License version 2.|
-> |__________________________________________________________|\n";

if (($#ARGV != 0) && ($#ARGV != 2)) {
  print "USAGE: $0 <polynomial file | number file> [ id  num]\n";
  print "  where <polynomial file> is a file with the poly info to use\n";
  print "  or <number file> is a file containing the number to factor.\n";
  print "  Optional: id/num specifies that this is client <id> of <num>\n";
  print "            clients total (clients are numbered 1,2,3,...).\n";
  print " (Multi-client mode is still very experimental - it should only\n";
  print "  be used for testing, and is only intended as a hack for running\n";
  print "  conveniently on a very small number of machines - perhaps 5 - 10).\n";
  exit;
}
$NAME=$ARGV[0];
if ($#ARGV == 2) {
  $CLIENT_ID=$ARGV[1];
  $NUM_CLIENTS=$ARGV[2];
  if (($CLIENT_ID < 1) || ($CLIENT_ID > $NUM_CLIENTS)) {
    print "-> Error: client id should be between 1 and the number of clients ($NUM_CLIENTS)\n";
    exit -1;
  }
  $PNUM += $CLIENT_ID;
} else {
  # Single client.
  $NUM_CLIENTS=1;
  $CLIENT_ID=1;
}

printf "-> This is client $CLIENT_ID of $NUM_CLIENTS\n";
printf "-> Using $NUM_CPUS threads\n";

$NAME =~ s/\.poly//;
$NAME =~ s/\.n//;
print "-> Working with NAME=$NAME...\n";
$JOBNAME=$NAME.".job";
$ININAME=$NAME.".ini";
$DATNAME=$NAME.".dat";
$FBNAME=$NAME.".fb";
$DEPFILE=$DATNAME.".dep";
$COLS=$DATNAME.".cyc";
$SIEVER_OUTPUTNAME="spairs.out";
$SIEVER_ADDNAME="spairs.add";
if ($CLIENT_ID > 1) {
  $JOBNAME .= ".$CLIENT_ID";
  $SIEVER_OUTPUTNAME .= "$CLIENT_ID";
  $SIEVER_ADDNAME .= ".$CLIENT_ID";
}
$psTime=0;

# Is there a poly file already, or do we need to create one?
if (!(-e $NAME.".poly")) {
  printf("-> Error: Polynomial file $NAME.poly does not exist!\n");
  if ($NUM_CLIENTS > 1) {
    printf "-> Script does not support polynomial search across multiple clients!\n";
    exit ;
  }
  unlink $PARAMFILE;
  if (-e $NAME.".n") {
    open(IF, "$NAME".".n");
    my @numberinf=<IF>;
    close(IF);
    my @TMP=grep(/^n:/, @numberinf);
    $TMP[0] =~ /n:\s*(\d+)/;
    $N=$1;
    if (length($N)>0) {
      printf("-> Found n=$N.\n");
      die ("-> Error:  n is probably prime!\n") if (probab_prime_p($N, 10));
      printf("-> Attempting to run polyselect...\n");
      if (length($N)<98) { $USE_KLEINJUNG_FRANKE_PS=0; }
      $psTime=time;
      if ($USE_KLEINJUNG_FRANKE_PS) {
        runPol5;
      } else {
        runPolyselect;
      }
      $psTime=(time-$psTime)/3600;
      die("Polynomial selection failed.\n") unless (-e $NAME.".poly");
    } else {
      printf("-> Could not find a number in the file $NAME.n\n");
      printf("-> Did you forget the 'n:' tag?\n");
      exit;
    }
  }
}

# Read and verify parameters for the factorization.
if (!(-e $NAME.".poly")) {
  die("Cannot find $NAME.poly\n");
}
readParams;
checkParams;
if (!(-e $DEPFILE)) { setup; }

# Do some classical sieving, if needed/applicable.
# This is broken - it is still a work in progress!
$DOCLASSICAL = 0 if ($CLIENT_ID > 1);
if ($DOCLASSICAL) {
  classicalSieve($classicalA, 1, $classicalB);
}

####################################################
# Finally, sieve until                             #
# we have reached the desired min # of FF's.       #
####################################################
$sieveSideOpt = ($LATSIEVE_SIDE) ? '-r' : '-a';
$sieveSide = ($LATSIEVE_SIDE) ? 'rational' : 'algebraic';
while (!(-e $COLS)) {
  printf "-> Q0=$Q0, QSTEP=$QSTEP.\n";

  # Create a job file.
  makeJobFile($JOBNAME, $Q0, $QSTEP, $CLIENT_ID, $NUM_CLIENTS);

  printf("-> Lattice sieving $sieveSide q-values from q=$Q0 to %d.\n", $Q0+$thisQRSize);
  if ($Q0 >= 2**$LPBA) {
    printf "-> $0 : Severe error!\n";
    printf("->     Current special q=$Q0 has exceeded max. large alg. prime = %d !\n", 2**$LPBA);
    printf("-> You can try increasing LPBA, re-launch this script and cross your fingers.\n");
    printf("-> But be aware that if you're seeing this, your factorization is taking\n");
    printf("-> much longer than it would have with better parameters.\n");
    exit;
  }
  $startTime = time;

  open(OF, ">>$LOGFILE");
  print OF "->               client $CLIENT_ID q0: $Q0\n";
  close(OF);
  
  if ($NUM_THREADS==1) {
    # It's very important to call like this, so that if the user CTRL-C's,
    # or otherwise kills the process, we see it and terminate as well.
    unlink($SIEVER_OUTPUTNAME);
    $cmd="$NICE \"$LATSIEVER\" -k -o $SIEVER_OUTPUTNAME -v -n$PNUM $sieveSideOpt $JOBNAME";
    print "=>$cmd\n" if($ECHO_CMDLINE);
    $res=system($cmd);
    # If we are sieving below the AFB limit, we need to delete the
    # siever's factor base file to make it create a new one. This is
    # a dirty hack - what we should do is modify Franke's code to
    # explicitly allow it.
    if ($LATSIEVE_SIDE) {
      unlink <$JOBNAME.afb.1> if ($sieverRL == ($Q0-1));
    }
    else {
      unlink <$JOBNAME.afb.0> if ($sieverAL == ($Q0-1));
    }
  }
  else {
    @thd = (1 .. $NUM_THREADS);
    for ($j=1;$j<=$NUM_THREADS;$j++) {
      unlink("$SIEVER_OUTPUTNAME.T$j");
      $cmd="$NICE \"$LATSIEVER\" -k -o $SIEVER_OUTPUTNAME.T$j -v -n$PNUM $sieveSideOpt $JOBNAME.T$j";
#     $cmd="taskset -c ".($j-1)." $NICE \"$LATSIEVER\" -k -o $SIEVER_OUTPUTNAME.T$j -v -n$PNUM $sieveSideOpt $JOBNAME.T$j";
#           taskset can be used on Linux systems
      print "=>$cmd\n" if($ECHO_CMDLINE);
      $thd[$j]=async{system($cmd)};
    }
    $res=0;
    for ($j=1;$j<=$NUM_THREADS;$j++) {
      $res+=eval{$thd[$j]->join()};
    }
    for ($j=1;$j<=$NUM_THREADS;$j++) {
      if ($LATSIEVE_SIDE) {
        unlink <$JOBNAME.T$j.afb.1> if ($sieverRL == ($Q0-1));
      }
      else {
        unlink <$JOBNAME.T$j.afb.0> if ($sieverAL == ($Q0-1));
      }
      $cmd="\"$CAT\" $SIEVER_OUTPUTNAME.T$j >> $SIEVER_OUTPUTNAME";
      print "=>$cmd\n" if($ECHO_CMDLINE);
      concat("$SIEVER_OUTPUTNAME.T$j", ">>", "$SIEVER_OUTPUTNAME");
      unlink "$SIEVER_OUTPUTNAME.T$j";
    }
  }
  if ($res) {
    print "-> Return value $res. Updating job file and terminating...\n";
    open(INFO, ".last_spq$PNUM");
    $lastSPQ=<INFO>;
    chomp $lastSPQ;
    close(INFO);
    unlink ".last_spq$PNUM";
    if ($lastSPQ < $Q0) {
      $lastSPQ = $Q0;
    }
    # Move the new relations so they won't be wiped on restart.
    $cmd="\"$CAT\" $SIEVER_OUTPUTNAME >> $SIEVER_ADDNAME";
    print "=>$cmd\n" if($ECHO_CMDLINE);
    concat("$SIEVER_OUTPUTNAME", ">>", "$SIEVER_ADDNAME");
    unlink "$SIEVER_OUTPUTNAME";
    # And update the job file accordingly:
    makeJobFile($JOBNAME, $lastSPQ, $QSTEP, $CLIENT_ID, $NUM_CLIENTS);
    # Record the time to the logfile.
    $stopTime = time;
    $totalTime = $stopTime - $startTime;
    die "Terminating...\n";
  }

  die("Some error ocurred and no relations were found! Examing log file.\n") unless
      -e "$SIEVER_OUTPUTNAME";

  $Q0=$Q1+1;
  if ($CLIENT_ID > 1) {
    $cmd="\"$CAT\" $SIEVER_OUTPUTNAME >> spairs.add.$CLIENT_ID";
    print "=>$cmd\n" if($ECHO_CMDLINE);
    concat($SIEVER_OUTPUTNAME, ">>", "spairs.add.$CLIENT_ID");
  } else {
    # Are there relations coming from somewhere else which should be added in?
    if (-e "spairs.add") {
      $cmd="\"$CAT\" spairs.add >> spairs.out";
      print "=>$cmd\n" if($ECHO_CMDLINE);
      concat("spairs.add", ">>", "spairs.out");
      unlink "spairs.add";
    }
    for ($i=1; $i<=$NUM_CLIENTS; $i++) {
      if (-e "spairs.add.$i") {
        $cmd="\"$CAT\" spairs.add.$i >> spairs.out";
        print "=>$cmd\n" if($ECHO_CMDLINE);
	concat("spairs.add.$i", ">>", "spairs.out");
        unlink "spairs.add.$i";
      }
    }
    $stopTime = time;
    $totalTime = $stopTime - $startTime;
    
    if(-e "MINRELS.txt") {
      open IN,"<MINRELS.txt";
      my $q = <IN>;
      $q =~ /(\d+)/;
      $MINRELS=$1 if($1);
      close IN;
    }
    $cmd="\"$CAT\" spairs.out >> $DATNAME";
    print "=>$cmd\n" if($ECHO_CMDLINE);
    concat("spairs.out", ">>", "$DATNAME");
    $CURR_RELS = linecount;
    print "Found $CURR_RELS relations, need at least $MINRELS to proceed.\n";
    if ($CURR_RELS > $MINRELS) {
      $cmd="$NICE \"$MSIEVE\" -s $DATNAME -l $LOGFILE -i $ININAME -v -nf $FBNAME -t $NUM_CPUS -nc1";
      print "=>$cmd\n" if($ECHO_CMDLINE);
      $res=system($cmd);
      die "Return value $res. Terminating...\n" if ($res);
    }
    if ($SAVEPAIRS) {
      $cmd="$NICE \"$GZIP\" -c spairs.out >> spairs.save.gz"; 
      print "=>$cmd\n" if($ECHO_CMDLINE);
      $res=system($cmd);
      die "Return value $res. Terminating...\n" if ($res);
    }
    unlink "spairs.out";
    unlink $JOBNAME;
  }
}

if ($CLIENT_ID > 1) {
  printf "Client $CLIENT_ID terminating...\n";
  exit 0;
}

#######################################
# Obviously, the matrix solving step. #
#######################################
if (!(-e $DEPFILE)) {
  print "-> Doing matrix solving step...\n";
  $cmd="$NICE \"$MSIEVE\" -s $DATNAME -l $LOGFILE -i $ININAME -v -nf $FBNAME -t $NUM_CPUS -nc2";
  print "=>$cmd\n" if($ECHO_CMDLINE);
  $res=system($cmd);
  die "Return value $res. Terminating...\n" if ($res);
  die("Some error occurred and matsolve did not record dependencies.\n") unless
     (-e $DEPFILE);
} else {
  printf "-> File 'deps' already exists. Proceeding to sqrt step.\n";
}
#############################################
# Do as many square root jobs as needed to  #
# get the final factorization.              #
#############################################
$cmd="$NICE \"$MSIEVE\" -s $DATNAME -l $LOGFILE -i $ININAME -v -nf $FBNAME -t $NUM_CPUS -nc3";
print "=>$cmd\n" if($ECHO_CMDLINE);
$res=system($cmd);

if ($CLEANUP) {
  unlink $LOGFILE, <cols*>, <deps*>, 'factor.easy', <lpindex*>;
  unlink <rels*>, 'spairs.out', 'spairs.save.gz', $NAME.'.fb', <*.afb.0>;
  unlink "tmpdata.000";
  unlink $PARAMFILE;
}

# Figure the time scale for this machine.
printf "-> Computing time scale for this machine...\n";
@TMP=`"$PROCRELS" -speedtest`;
@TMP=grep(/timeunit:/,@TMP);
$TIMESCALE=$TMP[0];
$TIMESCALE =~ s/timeunit: //;

# And gather up some stats.
$sieveT=0.0;
$relprocT=0.0;
open(INFO,$LOGFILE);
while (<INFO>) {
  s/\s+$//;
  @_ = split;
  if ($_[0] eq "LatSieveTime:") { $sieveT += $_[1]; }
  if (/(Msieve.*)$/) { $version = $1; }
  if (/RelProcTime: (\S+)/) { $relprocT += $1; }
  if (/BLanczosTime: (\S+)/) { $matT += $1; }
  if (/sqrtTime: (\S+)/) { $sqrtT += $1; }
  if (/rational ideals/) { $rprimes=$_[6]." ".$_[7]." ".$_[5]; }
  if (/algebraic ideals/) { $aprimes=$_[6]." ".$_[7]." ".$_[5]; }
#  if (/largePrimes:/) { $lprimes=$_[2].$_[3].' encountered'; }
  if (/unique relations/) { $rels=$_[9]." ".$_[11]; }
#  if (/Initial matrix/) { s/\[.*\] Initial matrix is //; $initmat=$_; }
  if (/matrix is/) { $prunedmat=$_[7]." x ".$_[9]; }
  if (/prp/) {
    s/.*factor: //;
    if ((length($_) > 1) && (length($_) < length($N))) {
      push(@DIVISORS, $_);
    }
  }

}
close(INFO);
# Convert times from seconds to hours.
$sieveT /= 3600.0; $relprocT /= 3600.0;
$matT /= 3600.0;   $sqrtT /= 3600.0;
$totalT=$sieveT+$relprocT+$matT+$sqrtT+$psTime;

open($std, ">&STDOUT");         # Save STDOUT handle

if ($TYPE =~ /gnfs/) {
  my $tmpL = length($N);
  my ($dname, $lname) = ($NAME =~ m|(.*/)?(.+)|);
  $sumName="${dname}g${tmpL}-${lname}.txt";
} else {
  my $tmpL=$SNFS_DIFFICULTY->bfloor();
  my ($dname, $lname) = ($NAME =~ m|(.*/)?(.+)|);
  $sumName="${dname}s${tmpL}-${lname}.txt";
}

printf "sumName = $sumName\n";
open(STDOUT, ">$sumName");
print "Number: $NAME\n";
print "N=$N\n";
printf("  ( %d digits)\n", length($N));
if ($TYPE =~ /snfs/) { 
  printf("SNFS difficulty: %d digits.\n", $SNFS_DIFFICULTY);
}
print "Divisors found:\n";
print (" knowndiv: $KNOWNDIV\n") if $KNOWNDIV;

# Sort ascending numerically
@DIVISORS = sort {$a <=> $b} (@DIVISORS);
$r = 1;
while($_ = shift @DIVISORS) {
  printf(" r%d=%s (pp%d)\n", $r++, $_, length($_));
}
$version = "Msieve-1.40" unless $version;
printf("Version: $version\n");
printf("Total time: %1.2f hours.\n", $totalT);
printf("Scaled time: %1.2f units (timescale=%1.3lf).\n", $totalT*$TIMESCALE,$TIMESCALE);
print "Factorization parameters were as follows:\n";
open(PARS, "$NAME.poly");
while (<PARS>) { print $_; }
close(PARS);
print "Factor base limits: $RLIM/$ALIM\n";
print "Large primes per side: $LARGEP\n";
print "Large prime bits: $LPBR/$LPBA\n";
print "Max factor residue bits: $MFBR/$MFBA\n";
print "Sieved $sieveSide special-q in [$QSTART, $Q0)\n";
print "Primes: $rprimes, $aprimes, $lprimes\n";
print "Relations: $rels\n";
print "Max relations in full relation-set: $maxRelsInFF\n";
print "Initial matrix: $initmat\n";
print "Pruned matrix : $prunedmat\n";
if ($psTime > 0) {
  printf("Polynomial selection time: %1.2f hours.\n", $psTime);
}
printf("Total sieving time: %1.2f hours.\n", $sieveT);
printf("Total relation processing time: %1.2f hours.\n", $relprocT);
printf("Matrix solve time: %1.2f hours.\n", $matT);
printf("Time per square root: %1.2f hours.\n", $sqrtT);

if ($TYPE =~ /snfs/) { 
  $DIGS = $SNFS_DIFFICULTY->bfloor(); 
  $DEFLINE="$TYPE,$DIGS,$DEGREE,0,0,0,0,0,0,0,0,$RLIM,$ALIM,$LPBR,$LPBA,$MFBR,$MFBA,$RLAMBDA,$ALAMBDA,$QINTSIZE\n";
}
else {
  $DIGS=int(log($N)/log(10.0));
  $DEFLINE="$TYPE,$DIGS,$DEGREE,maxs1,maxskew,goodScore,efrac,j0,j1,eStepSize,maxTime,$RLIM,$ALIM,$LPBR,$LPBA,$MFBR,$MFBA,$RLAMBDA,$ALAMBDA,$QINTSIZE\n";
}
printf("Prototype def-par.txt line would be:\n$DEFLINE");
printf("total time: %1.2f hours.\n", $totalT);
print " --------- CPU info (if available) ----------\n";
close(STDOUT);


if (-x '/bin/dmesg') {
  $cmd="/bin/dmesg | grep CPU | grep stepping >> $sumName";
  print "=>$cmd\n" if($ECHO_CMDLINE);
  system($cmd);
  $cmd="/bin/dmesg | grep Memory >> $sumName";
  print "=>$cmd\n" if($ECHO_CMDLINE);
  system($cmd);
  $cmd="/bin/dmesg | grep -i bogomips >> $sumName";
  print "=>$cmd\n" if($ECHO_CMDLINE);
  system($cmd);
}
if (-x '/usr/sbin/x86info') {
  $cmd="/usr/sbin/x86info -mhz >> $sumName";
  print "=>$cmd\n" if($ECHO_CMDLINE);
  system($cmd);
}
if (-x '/usr/sbin/system_profiler') {
   $cmd="/usr/sbin/system_profiler | head -n 12 | tail -n 8 >> $sumName";
   print "=>$cmd\n" if($ECHO_CMDLINE);
   system($cmd);
}

open(STDOUT, ">&", $std); # Restore orig STDOUT
print "-> Factorization summary written to $sumName.\n";

# Send 5 BELLS and flush output so \n not necessary (in Cygwin) after each BELL
use IO::Handle;
STDOUT->autoflush(1);

for($i = 0; $i < 5; $i++){sleep 1;print "\a";}



