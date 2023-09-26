#include <mach/mach_time.h>

static mach_timebase_info_data_t timebase;

void initialize_mach_timebase_info() {
  mach_timebase_info(&timebase);
}

uint64_t get_time_in_ns() {
  uint64_t time = mach_absolute_time();
  return time * timebase.numer / timebase.denom;
}
